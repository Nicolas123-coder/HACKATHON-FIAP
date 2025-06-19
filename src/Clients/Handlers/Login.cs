using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Utils;
using Clients.DTOs;
using Clients.Entities;
using Clients.Validators;

namespace Clients.Handlers
{
    public class Login
    {
        private readonly DynamoDbHelper _db;
        private readonly PasswordHasherService _hasher;
        private readonly string _tableName;
        private readonly string _jwtSecret;

        public Login(DynamoDbHelper db, PasswordHasherService hasher)
        {
            _db         = db;
            _hasher     = hasher;
            _tableName  = Environment.GetEnvironmentVariable("CLIENTS_TABLE")
                          ?? throw new InvalidOperationException("CLIENTS_TABLE não definido");
            _jwtSecret  = Environment.GetEnvironmentVariable("JWT_SECRET")
                          ?? throw new InvalidOperationException("JWT_SECRET não definido");
        }

        public async Task<APIGatewayProxyResponse> HandleAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            context.Logger.LogLine("[Login] Iniciando HandleAsync");
            context.Logger.LogLine($"Payload recebido: {request.Body}");

            LoginRequestDTO dto;
            try
            {
                dto = JsonSerializer.Deserialize<LoginRequestDTO>(request.Body)
                      ?? throw new JsonException("Body vazio");
            }
            catch (JsonException ex)
            {
                context.Logger.LogLine($"Erro de desserialização JSON: {ex.Message}");
                return Response.BadRequest("Formato de JSON inválido.");
            }
            
            var errors = LoginValidator.Validate(dto);
            if (errors.Count > 0)
            {
                return Response.BadRequest(string.Join(" ", errors));
            }

            var users = await _db.ScanAsync<Client>(new List<ScanCondition>
            {
                new ScanCondition("Email", ScanOperator.Equal, dto.Login)
            });
            var user = users.FirstOrDefault();

            if (user == null)
            {
                var byCpf = await _db.ScanAsync<Client>(new List<ScanCondition>
                {
                    new ScanCondition("CPF", ScanOperator.Equal, dto.Login)
                });
                user = byCpf.FirstOrDefault();
            }

            if (user == null)
            {
                context.Logger.LogLine("Usuário não encontrado.");
                return Response.Unauthorized("Credenciais inválidas.");
            }

            var verify = _hasher.VerifyHashedPassword(
                null!, user.PasswordHash, dto.Password);
            if (verify == PasswordVerificationResult.Failed)
            {
                context.Logger.LogLine("Senha incorreta.");
                return Response.Unauthorized("Credenciais inválidas.");
            }

            var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim("cpf",                     user.CPF)
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject            = new ClaimsIdentity(claims),
                Expires            = DateTime.UtcNow.AddHours(2),
                SigningCredentials = creds
            };
            var tokenHandler    = new JwtSecurityTokenHandler();
            var securityToken   = tokenHandler.CreateToken(tokenDescriptor);
            var jwt             = tokenHandler.WriteToken(securityToken);

            var responseDto = new LoginResponseDTO
            {
                Token = $"Bearer {jwt}"
            };
            return Response.Ok(responseDto);
        }
    }
}
