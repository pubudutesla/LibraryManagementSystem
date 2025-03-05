using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

public interface IAuthService
{
    Task<string?> Authenticate(string username, string password);
}

namespace LibraryManagementSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<string?> Authenticate(string username, string password)
        {
            var member = await _unitOfWork.Members.GetByUsernameAsync(username);

            if (member == null || !VerifyPassword(password, member.PasswordHash))
                return null;

            return GenerateJwtToken(member);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return storedHash == HashPassword(password); // Compare hash of input password
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateJwtToken(Member member)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, member.Username),
                new Claim(ClaimTypes.Role, member.MembershipType.ToString()),
                //new Claim("Permission", "ManageLoans")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:ExpiryMinutes"])),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}