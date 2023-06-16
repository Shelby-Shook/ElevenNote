using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElevenNote.Data.Entities;
using ElevenNote.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ElevenNote.Models.Tokens;
using ElevenNote.Models.Token;
using Microsoft.AspNetCore.Identity;

namespace ElevenNote.Services.Token;

    public class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        public TokenService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public async Task<TokenResponse?> GetTokenAsync(TokenRequest model) 
        { 
            UserEntity? entity = await GetValidUserAsync(model);

            if (entity is null)
                return null;

            return GenerateToken(entity);
        }
        private async Task<UserEntity?> GetValidUserAsync(TokenRequest model) 
        { 
            UserEntity? entity = await _context.Users.FirstOrDefaultAsync(user => user.Username.ToLower() == model.Username.ToLower());
            if (entity is null)
                return null;

            var passwordHasher = new PasswordHasher<UserEntity>();
            var verifyPasswordResult = passwordHasher.VerifyHashedPassword(entity, entity.Password, model.Password);
            if (verifyPasswordResult == PasswordVerificationResult.Failed)
                return null;

            return entity;
        }

        
        private TokenResponse GenerateToken(UserEntity entity) 
        {
            var claims = GetClaims(entity);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? ""));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                Subject = new ClaimsIdentity(claims),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(14),
                SigningCredentials = credentials
            };

            return res;
        }
    }

    private Claim[] GetClaims(UserEntity user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        var name = !string.IsNullOrWhiteSpace(fullName) ? fullName : user.Username;

        var claims = new Claim[]
        {
            new("Id", user.Id.ToString()),
            new("Username", user.Username),
            new("Email", user.Email),
            new("Name", name),
        };

        return claims;
    }