using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyWebApplication
{
    public class Token
    {
        internal static string CreateToken(Person person)
        {
            string token = string.Empty;

            var claims = new List<Claim> {
        new Claim(ClaimTypes.Name, person.Name),
        new Claim("Age", person.Age.ToString()),
        new Claim("isBlocked", person.IsBlocked.ToString()),
        new Claim("Id", person.Id),
        new Claim("Password", person.Password),
        new Claim(ClaimTypes.Email, person.Email) };
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            token = encodedJwt;
            return token;
        }
    }
}
