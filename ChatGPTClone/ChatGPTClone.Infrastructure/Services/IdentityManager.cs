﻿using System.Web;
using ChatGPTClone.Application.Common.Interfaces;
using ChatGPTClone.Application.Common.Models.Identity;
using ChatGPTClone.Application.Common.Models.Jwt;
using ChatGPTClone.Application.Features.Auth.Commands.RefreshToken;
using ChatGPTClone.Domain.Entities;
using ChatGPTClone.Domain.Settings;
using ChatGPTClone.Infrastructure.Identity;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChatGPTClone.Infrastructure.Services;

public class IdentityManager : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;


    public IdentityManager(UserManager<AppUser> userManager, IJwtService jwtService, IOptions<JwtSettings> jwtSettings, IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _currentUserService = currentUserService;
    }

    // Kullanıcının kimliğini doğrular.
    public async Task<bool> AuthenticateAsync(IdentityAuthenticateRequest request, CancellationToken cancellationToken)
    {
        // Kullanıcıyı e-posta adresine göre bul.
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Kullanıcı bulunamazsa false döndür.
        if (user is null) return false;

        // Kullanıcının parolasını kontrol et ve sonucu döndür.
        return await _userManager.CheckPasswordAsync(user, request.Password);
    }

    // E-posta adresinin veritabanında olup olmadığını kontrol eder.
    public Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return _userManager
        .Users
        .AnyAsync(x => x.Email == email, cancellationToken);
    }

    public Task<bool> CheckIfEmailVerifiedAsync(string email, CancellationToken cancellationToken)
    {
        return _userManager
        .Users
        .AnyAsync(x => x.Email == email && x.EmailConfirmed, cancellationToken);
    }

    public async Task<bool> CheckSecurityStampAsync(Guid userId, string securityStamp, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return string.Equals(securityStamp, user.SecurityStamp);
    }

    public async Task<IdentityCreateEmailTokenResponse> CreateEmailTokenAsync(IdentityCreateEmailTokenRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        return new IdentityCreateEmailTokenResponse(token);
    }

    // Kullanıcının giriş yapmasını sağlar.
    public async Task<IdentityLoginResponse> LoginAsync(IdentityLoginRequest request, CancellationToken cancellationToken)
    {
        // Kullanıcıyı e-posta adresine göre bul.
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Kullanıcının rollerini al.
        var roles = await _userManager.GetRolesAsync(user);

        // JWT oluşturma isteği oluştur.
        var jwtRequest = new JwtGenerateTokenRequest(user.Id, user.Email, roles);

        // JWT oluştur.
        var jwtResponse = _jwtService.GenerateToken(jwtRequest);

        // Refresh token oluştur.
        var refreshToken = await CreateRefreshTokenAsync(user, cancellationToken);

        // Giriş yanıtını döndür.
        return new IdentityLoginResponse(jwtResponse.Token, jwtResponse.ExpiresAt, refreshToken.Token, refreshToken.Expires);
    }

    public async Task<IdentityRefreshTokenResponse> RefreshTokenAsync(IdentityRefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserIdFromJwt(request.AccessToken);

        // Kullanıcıyı ID'sine göre bul.
        var user = await _userManager.FindByIdAsync(userId.ToString());

        // _collection.FindOne

        // Kullanıcının rollerini al.
        var roles = await _userManager.GetRolesAsync(user);

        // JWT oluşturma isteği oluştur.
        var jwtRequest = new JwtGenerateTokenRequest(user.Id, user.Email, roles);

        // JWT oluştur.
        var jwtResponse = _jwtService.GenerateToken(jwtRequest);

        // Giriş yanıtını döndür.
        return new IdentityRefreshTokenResponse(jwtResponse.Token, jwtResponse.ExpiresAt);
    }

    // Yeni bir kullanıcı kaydeder.
    public async Task<IdentityRegisterResponse> RegisterAsync(IdentityRegisterRequest request, CancellationToken cancellationToken)
    {
        // Yeni bir kullanıcı kimliği oluştur.
        var userId = Ulid
        .NewUlid()
        .ToGuid();

        // Yeni bir kullanıcı nesnesi oluştur.
        var user = new AppUser
        {
            Id = userId,
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedByUserId = userId.ToString(),
            CreatedOn = DateTimeOffset.UtcNow,
            EmailConfirmed = false,
        };

        // Kullanıcıyı veritabanına kaydet.
        var result = await _userManager.CreateAsync(user, request.Password);

        // Kayıt işlemi başarısız olursa hata fırlat.
        if (!result.Succeeded) CreateAndThrowValidationException(result.Errors);

        // E-posta onaylama jetonu oluştur.
        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Kayıt yanıtını döndür.
        return new IdentityRegisterResponse(userId, user.Email, emailToken);
    }

    public async Task<IdentityVerifyEmailResponse> VerifyEmailAsync(IdentityVerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // var decodedToken = HttpUtility.UrlDecode(request.Token);

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);

        if (!result.Succeeded)
            CreateAndThrowValidationException(result.Errors);

        return new IdentityVerifyEmailResponse(user.Email);
    }

    // Doğrulama hatası oluşturur ve fırlatır.
    private void CreateAndThrowValidationException(IEnumerable<IdentityError> errors)
    {
        // Hata mesajlarını ve özelliklerini içeren yeni bir doğrulama hatası oluştur.
        var errorMessages = errors
        .Select(x => new ValidationFailure(x.Code, x.Description))
        .ToArray();

        // Doğrulama hatasını fırlat.
        throw new ValidationException(errorMessages);
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(AppUser user, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            CreatedByUserId = user.Id.ToString(),
            CreatedOn = DateTimeOffset.UtcNow,
            AppUserId = user.Id,
            Token = Ulid.NewUlid().ToString(), // Rastegele token oluştur.
            Expires = DateTime.UtcNow.Add(_jwtSettings.RefreshTokenExpiration), // Refresh tokenın süresini belirler.
            SecurityStamp = user.SecurityStamp, // Kullanıcının güvenlik damgasını kullanır.
            CreatedByIp = _currentUserService.IpAddress, // İp adresini kullanır.
        };

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }
}