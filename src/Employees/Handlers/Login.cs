using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2.DataModel;
using Employees.DTOs;
using Employees.Entities;
using Utils;
using Microsoft.IdentityModel.Tokens;
using Amazon.DynamoDBv2.DocumentModel;

namespace Employees.Handlers
{
    public class Login
    {
        private readonly DynamoDbHelper _db;
        private readonly PasswordHasherService _hasher;

        public Login(DynamoDbHelper db, PasswordHasherService hasher)
        {
            _db      = db;
            _hasher  = hasher;
        }

        public async Task<APIGatewayProxyResponse> HandleAsync(
            APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            context.Logger.LogLine("[LoginHandler] Iniciando HandleAsync");
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
                context.Logger.LogLine($"Validação falhou: {string.Join("; ", errors)}");
                return Response.BadRequest(string.Join(" ", errors));
            }

            // Busca usuário por email (precisa de GSI ou scan)
            var users = await _db.ScanAsync<Employee>(
                new List<ScanCondition>
                {
                    new ScanCondition("Email", ScanOperator.Equal, dto.Email)
                });
            var user = users.Count > 0 ? users[0] : null;
            if (user == null)
                return Response.Unauthorized("Credenciais inválidas.");

            // Verifica senha
            var verify = _hasher.VerifyHashedPassword(null!, user.PasswordHash, dto.Password);
            if (verify == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
                return Response.Unauthorized("Credenciais inválidas.");

            // Gera JWT
            var secret = Environment.GetEnvironmentVariable("JWT_SECRET")
                         ?? throw new InvalidOperationException("JWT_SECRET não definido");
            
            var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("role", user.Role.ToString())
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject            = new ClaimsIdentity(claims),
                Expires            = DateTime.UtcNow.AddHours(2),
                SigningCredentials = creds
            };
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(tokenDescriptor);
            var jwt = handler.WriteToken(securityToken);

            // Retorna o token
            var responseDto = new LoginResponseDTO { Token = $"Bearer {jwt}" };
            return Response.Ok(responseDto);
        }
    }
}
