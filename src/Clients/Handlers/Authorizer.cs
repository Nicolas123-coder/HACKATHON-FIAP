using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.IdentityModel.Tokens;

namespace Clients.Handlers
{
    public class Authorizer
    {
        private readonly string _secret;

        public Authorizer()
        {
            _secret = Environment
                .GetEnvironmentVariable("JWT_SECRET")
                ?? throw new InvalidOperationException("JWT_SECRET não definido");
        }

        /// <summary>
        /// Valida presença e validade do JWT no header Authorization
        /// e garante que exista a claim "cpf".
        /// </summary>
        public ClaimsPrincipal ValidateClient(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("[Authorizer] Iniciando validação de token de cliente");

            // 1) Captura header
            if (request.Headers == null ||
                (!request.Headers.TryGetValue("authorization", out var authHeader) &&
                 !request.Headers.TryGetValue("Authorization", out authHeader)))
            {
                throw new UnauthorizedAccessException("Authorization header ausente");
            }

            // 2) Extrai token
            var token = authHeader.Split(' ').LastOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token mal formado");
            }

            // 3) Configura validação
            var tokenHandler = new JwtSecurityTokenHandler();
            var key          = Encoding.UTF8.GetBytes(_secret);
            var parameters   = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(key),
                ValidateIssuer           = false,
                ValidateAudience         = false,
                ClockSkew                = TimeSpan.Zero
            };

            // 4) Valida e pega o principal
            try
            {
                var principal = tokenHandler.ValidateToken(token, parameters, out _);

                // 5) Confirma claim "cpf"
                var cpf = principal.Claims
                    .FirstOrDefault(c => c.Type == "cpf")?
                    .Value;

                if (string.IsNullOrEmpty(cpf))
                {
                    context.Logger.LogLine("[Authorizer] Claim 'cpf' ausente");
                    throw new UnauthorizedAccessException("Claim 'cpf' ausente");
                }

                context.Logger.LogLine($"[Authorizer] Cliente autenticado (cpf={cpf})");

                return principal;
            }
            catch (Exception ex) when (
                ex is SecurityTokenException ||
                ex is ArgumentException)
            {
                context.Logger.LogLine($"[Authorizer] Token inválido: {ex.Message}");
                throw new UnauthorizedAccessException("Token inválido");
            }
        }
    }
}
