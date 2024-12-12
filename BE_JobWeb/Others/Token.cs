
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BE_JobWeb.PasswordHasher
{
    public class Token
    {
        public string GenerateToken(string name, string role, string userid)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("qwaszxerdfcvtyghbnuijkmolpqdgrch");  // Khóa bí mật để mã hóa token

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                        new Claim(ClaimTypes.Name, name),
                        new Claim("userId", userid),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, role)
                    }),
                Expires = DateTime.UtcNow.AddHours(1),  // Token có hiệu lực trong 1 giờ
                Issuer = "http://localhost:5281",
                Audience = "http://localhost:5281",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}

//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace BE_JobWeb.PasswordHasher
//{
//    public class Token
//    {
//        private readonly IConfiguration _configuration;

//        // Inject IConfiguration thông qua constructor
//        public Token(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public string GenerateToken(string name, string role, string userId)
//        {
//            var tokenHandler = new JwtSecurityTokenHandler();

//            // Lấy thông tin từ appsettings.json
//            var jwtSettings = _configuration.GetSection("jwt");
//            var secretKey = jwtSettings["SecretKey"];
//            var issuer = jwtSettings["Issuer"];
//            var audience = jwtSettings["Audience"];

//            // Mã hóa khóa bí mật
//            var key = Encoding.UTF8.GetBytes(secretKey);

//            // Cấu hình token
//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(new[]
//                {
//                    new Claim(ClaimTypes.Name, name),
//                    new Claim("userId", userId),
//                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//                    new Claim(ClaimTypes.Role, role)
//                }),
//                Expires = DateTime.UtcNow.AddHours(1), // Token có hiệu lực trong 1 giờ
//                Issuer = issuer,
//                Audience = audience,
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//            };

//            var token = tokenHandler.CreateToken(tokenDescriptor);

//            return tokenHandler.WriteToken(token);
//        }
//    }
//}
