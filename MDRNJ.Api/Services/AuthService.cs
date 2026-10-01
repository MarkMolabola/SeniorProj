using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Exceptions;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Auth;
using MDRNJ.Shared.DTOs.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class AuthService : IAuthService
    {
        public const int VerificationCodeLifetimeMinutes = 15;
        private const int ResendCooldownSeconds = 60;
        private const int MaxVerificationAttempts = 5;

        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailService _email;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext db, IConfiguration config, IEmailService email, ILogger<AuthService> logger)
        {
            _db = db;
            _config = config;
            _email = email;
            _logger = logger;
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already in use.");

            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                throw new InvalidOperationException("Username already taken.");

            var user = AddNewUser(dto.Email, dto.Username, dto.DisplayName);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _db.SaveChangesAsync();

            // The account exists even if the email fails; the user can tap "Resend code".
            await TrySendVerificationCodeAsync(user);

            return new RegisterResponseDto
            {
                Email = user.Email,
                Message = "We sent a 6-digit code to your email. Enter it to finish signing up."
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (user.PasswordHash is null)
                throw new UnauthorizedAccessException("This account uses Google sign-in. Tap \"Continue with Google\".");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.EmailVerified)
            {
                // Send a fresh code unless one went out very recently (e.g. right after registering).
                if (!IsInResendCooldown(user))
                    await TrySendVerificationCodeAsync(user);

                throw new EmailNotVerifiedException("Please verify your email. We sent a 6-digit code to your inbox.");
            }

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto dto)
        {
            const string invalid = "That code is invalid or has expired.";

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
                ?? throw new ArgumentException(invalid);

            if (user.EmailVerified)
                throw new InvalidOperationException("This email is already verified. Please sign in.");

            if (user.VerificationCodeHash is null || user.VerificationCodeExpiresAt < DateTime.UtcNow)
                throw new ArgumentException(invalid);

            if (user.VerificationAttempts >= MaxVerificationAttempts)
                throw new ArgumentException("Too many attempts. Tap \"Resend code\" to get a new one.");

            if (!CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(user.VerificationCodeHash),
                    HashVerificationCode(dto.Code)))
            {
                user.VerificationAttempts++;
                await _db.SaveChangesAsync();
                throw new ArgumentException(invalid);
            }

            user.EmailVerified = true;
            ClearVerificationCode(user);
            await _db.SaveChangesAsync();

            return BuildAuthResponse(user);
        }

        public async Task ResendVerificationAsync(ResendVerificationDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Don't reveal whether an account exists for this email.
            if (user is null || user.EmailVerified)
                return;

            if (IsInResendCooldown(user))
            {
                var wait = ResendCooldownSeconds - (int)(DateTime.UtcNow - user.VerificationCodeSentAt!.Value).TotalSeconds;
                throw new InvalidOperationException($"Please wait {Math.Max(wait, 1)} seconds before requesting another code.");
            }

            await SendVerificationCodeAsync(user);
        }

        public async Task<AuthResponseDto> GoogleSignInAsync(ClaimsPrincipal googleUser)
        {
            var googleId = googleUser.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Google didn't return an account ID.");
            var email = googleUser.FindFirstValue(ClaimTypes.Email)
                ?? throw new UnauthorizedAccessException("Google didn't return an email address.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
            if (user is null)
            {
                // Only trust the email for linking/creating accounts if Google has verified it,
                // otherwise someone could take over an existing account with an unverified address.
                var emailVerified = string.Equals(
                    googleUser.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase);
                if (!emailVerified)
                    throw new UnauthorizedAccessException("Your Google account's email isn't verified.");

                user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user is null)
                {
                    var username = await GenerateUniqueUsernameAsync(email);
                    var displayName = googleUser.FindFirstValue(ClaimTypes.Name)?.Trim() ?? "";
                    if (displayName.Length < 2) displayName = username;
                    if (displayName.Length > 50) displayName = displayName[..50];

                    user = AddNewUser(email, username, displayName);
                }

                // Linking to an existing email/password account is safe because Google verified the address.
                user.GoogleId = googleId;
                user.EmailVerified = true;
                ClearVerificationCode(user);
                await _db.SaveChangesAsync();
            }

            return BuildAuthResponse(user);
        }

        // ── Helpers ─────────────────────────────────────

        private UserEntity AddNewUser(string email, string username, string displayName)
        {
            var user = new UserEntity { Email = email, Username = username };

            _db.Users.Add(user);
            _db.UserProfiles.Add(new UserProfileEntity { UserId = user.UserId, DisplayName = displayName });
            _db.GameStats.Add(new GameStatsEntity { UserId = user.UserId });
            _db.CharacterStates.Add(new CharacterStateEntity { UserId = user.UserId });
            _db.ActivityScores.Add(new ActivityScoreEntity { UserId = user.UserId });

            return user;
        }

        private async Task<string> GenerateUniqueUsernameAsync(string email)
        {
            var baseName = new string(email.Split('@')[0]
                .Where(c => char.IsAsciiLetterOrDigit(c) || c == '_')
                .ToArray());
            if (baseName.Length < 3) baseName = "user";
            if (baseName.Length > 24) baseName = baseName[..24];

            var candidate = baseName;
            while (await _db.Users.AnyAsync(u => u.Username == candidate))
                candidate = $"{baseName}{RandomNumberGenerator.GetInt32(1000, 10000)}";

            return candidate;
        }

        private static bool IsInResendCooldown(UserEntity user) =>
            user.VerificationCodeSentAt is { } sentAt &&
            (DateTime.UtcNow - sentAt).TotalSeconds < ResendCooldownSeconds;

        private async Task SendVerificationCodeAsync(UserEntity user)
        {
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

            user.VerificationCodeHash = Convert.ToHexString(HashVerificationCode(code));
            user.VerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(VerificationCodeLifetimeMinutes);
            user.VerificationCodeSentAt = DateTime.UtcNow;
            user.VerificationAttempts = 0;
            await _db.SaveChangesAsync();

            await _email.SendVerificationCodeAsync(user.Email, code);
        }

        private async Task TrySendVerificationCodeAsync(UserEntity user)
        {
            try
            {
                await SendVerificationCodeAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
            }
        }

        private static void ClearVerificationCode(UserEntity user)
        {
            user.VerificationCodeHash = null;
            user.VerificationCodeExpiresAt = null;
            user.VerificationAttempts = 0;
        }

        private static byte[] HashVerificationCode(string code) => SHA256.HashData(Encoding.UTF8.GetBytes(code));

        private AuthResponseDto BuildAuthResponse(UserEntity user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(7);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("username", user.Username),
                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                },
                expires: expires,
                signingCredentials: creds);

            return new AuthResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = Guid.NewGuid().ToString(),
                ExpiresAt = expires,
                User = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl,
                    DateJoined = user.DateJoined,
                    IsAdmin = user.IsAdmin
                }
            };
        }
    }
}
