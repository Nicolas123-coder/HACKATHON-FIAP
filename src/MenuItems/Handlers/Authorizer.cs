using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.IdentityModel.Tokens;
using Utils.Components;

namespace MenuItems.Handlers
{
    public class Authorizer
    {
        private readonly string _secret;

        public Authorizer()
        {
            _secret = Environment.GetEnvironmentVariable("JWT_SECRET")
                      ?? throw new InvalidOperationException("JWT_SECRET não definido");
        }

        public void ValidateManager(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("[Authorizer] Iniciando validação de token");

            if (request.Headers == null ||
                !request.Headers.TryGetValue("authorization", out var authHeader) &&
                !request.Headers.TryGetValue("Authorization", out authHeader))
            {
                throw new UnauthorizedAccessException("Authorization header ausente");
            }

            var token = authHeader.Split(' ').LastOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token mal formado");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret);
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(key),
                ValidateIssuer           = false,
                ValidateAudience         = false,
                ClockSkew                = TimeSpan.Zero
            };

            ClaimsPrincipal principal;
            try
            {
                principal = tokenHandler.ValidateToken(token, parameters, out _);
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"[Authorizer] Token inválido: {ex.Message}");
                throw new UnauthorizedAccessException("Token inválido");
            }

            var roleClaim = principal.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")
                ?.Value;

            if (!Enum.TryParse<EmployeeRoles>(roleClaim, ignoreCase: true, out var role) || role != EmployeeRoles.Manager)
            {
                context.Logger.LogLine($"[Authorizer] Acesso negado para role: {roleClaim}");
                throw new UnauthorizedAccessException("Acesso negado: usuário não é Manager");
            }

            context.Logger.LogLine("[Authorizer] Autorização concedida para Manager");
        }
    }
}
