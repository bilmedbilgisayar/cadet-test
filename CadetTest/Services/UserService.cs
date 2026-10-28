using CadetTest.Entities;
using CadetTest.Helpers;
using CadetTest.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CadetTest.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
    }

    public class UserService : IUserService
    {
        private DataContext _context;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UserService> _logger;

        public UserService(DataContext context, IOptions<AppSettings> appSettings, ILogger<UserService> logger)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress = "127.0.0.1")
        {
            try
            {
                var user = _context.Users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);
                if (user == null) return null;

                var expirationDate = DateTime.UtcNow.AddMinutes(_appSettings.ExpirationInMinutes);

                var jwtToken = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken(ipAddress);
                user.RefreshTokens = new List<RefreshToken>();
                user.RefreshTokens.Add(refreshToken);
                _context.Update(user);
                _context.SaveChanges();

                return new AuthenticateResponse(user, jwtToken, expirationDate, refreshToken.Token);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw new Exception($"Hata: {e.Message}");
            }
        }

        // helper methods

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_appSettings.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddMinutes(_appSettings.ExpirationInMinutes),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }
    }
}
