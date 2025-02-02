using dqapi.Infrastructure.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace dqapi.Application.Common
{
    public class AuthHelper
    {
        private readonly AppDbDataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonHelper _jsonHelper;
        private readonly IConfiguration _config;

        public AuthHelper(AppDbDataContext dataContext, IHttpContextAccessor httpContextAccessor, JsonHelper jsonHelper, IConfiguration config)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _jsonHelper = jsonHelper;
            _config = config;
        }

        public virtual string GetAuthorizationToken()
        {
            return _httpContextAccessor?.HttpContext?.Request.Headers.Authorization.FirstOrDefault() ?? throw new Exception("Couldn't extract auth token.");
        }

        public virtual byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            );
        }

        public virtual byte[] GetPasswordSalt()
        {
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }
            return passwordSalt;
        }

        public virtual string CreateToken(string userUuid)
        {
            Claim[] claims = new[]
            {
                new Claim("userUuid", userUuid),
                new Claim("elementR", Guid.NewGuid().ToString())
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:TokenKey").Value ?? throw new Exception("couldn't extract Token Key.")));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(3),
                SigningCredentials = credentials
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        //public virtual Session CreateSession(string userUuid, string token)
        //{
        //    Session session = _dataContext.RequestSession(new SessionRequest()
        //    {
        //        UserUuid = userUuid,
        //        AuthToken = token
        //    });

        //    return session;
        //}
        public virtual string GetUserUuid()
        {
            return _httpContextAccessor?.HttpContext?.User.FindFirst("userUuid")?.Value.ToLower() ?? throw new Exception("Couldn't extract user UUID.");
        }
    }
}
