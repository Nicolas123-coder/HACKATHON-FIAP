using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.IdentityModel.Tokens;

namespace Orders.Handlers
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
        /// Valida presença e validade do JWT no header Authorization,
        /// e confirma que a claim 'role' é 'manager' ou 'employee'.
        /// </summary>
        public ClaimsPrincipal ValidateOrderAccess(
            APIGatewayProxyRequest request,
            ILambdaContext         context)
        {
            context.Logger.LogLine("[Authorizer] Iniciando validação de token para Orders");

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

            // 3) Configura parâmetros de validação
            var tokenHandler = new JwtSecurityTokenHandler();
            var key          = Encoding.UTF8.GetBytes(_secret);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(key),
                ValidateIssuer           = false,
                ValidateAudience         = false,
                ClockSkew                = TimeSpan.Zero
            };

            // 4) Valida token e obtém o principal
            ClaimsPrincipal principal;
            try
            {
                principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (Exception ex) when (
                ex is SecurityTokenException ||
                ex is ArgumentException)
            {
                context.Logger.LogLine($"[Authorizer] Token inválido: {ex.Message}");
                throw new UnauthorizedAccessException("Token inválido");
            }

            // 5) Verifica claim 'role'
            var roleClaim = principal.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")
                ?.Value;

            if (string.IsNullOrEmpty(roleClaim))
            {
                context.Logger.LogLine("[Authorizer] Claim 'role' ausente");
                throw new UnauthorizedAccessException("Role não informado no token");
            }

            if (!string.Equals(roleClaim, "manager", StringComparison.OrdinalIgnoreCase)
             && !string.Equals(roleClaim, "employee", StringComparison.OrdinalIgnoreCase))
            {
                context.Logger.LogLine($"[Authorizer] Role inválido: {roleClaim}");
                throw new UnauthorizedAccessException("Acesso negado: role não permitido");
            }

            context.Logger.LogLine($"[Authorizer] Autorizado (role={roleClaim})");
            return principal;
        }
    }
}
