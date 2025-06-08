using System.Collections.Generic;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace Utils
{
    public static class Response
    {
        private static readonly Dictionary<string, string> DefaultHeaders = new()
        {
            { "Content-Type", "application/json" }
        };

        /// <summary>
        /// Retorna 200 OK com o objeto serializado em JSON.
        /// </summary>
        public static APIGatewayProxyResponse Ok(object body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body       = JsonSerializer.Serialize(body),
                Headers    = DefaultHeaders
            };
        }

        /// <summary>
        /// Retorna 201 Created com o objeto serializado em JSON.
        /// </summary>
        public static APIGatewayProxyResponse Created(object body)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 201,
                Body       = JsonSerializer.Serialize(body),
                Headers    = DefaultHeaders
            };
        }

        /// <summary>
        /// Retorna 400 Bad Request com mensagem de erro.
        /// </summary>
        public static APIGatewayProxyResponse BadRequest(string errorMessage)
        {
            var body = new { message = errorMessage };
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body       = JsonSerializer.Serialize(body),
                Headers    = DefaultHeaders
            };
        }

        /// <summary>
        /// Retorna 401 Unauthorized com mensagem de erro.
        /// </summary>
        public static APIGatewayProxyResponse Unauthorized(string errorMessage)
        {
            var body = new { message = errorMessage };
            return new APIGatewayProxyResponse
            {
                StatusCode = 401,
                Body       = JsonSerializer.Serialize(body),
                Headers    = DefaultHeaders
            };
        }

        /// <summary>
        /// Retorna 404 Not Found com mensagem de erro.
        /// </summary>
        public static APIGatewayProxyResponse NotFound(string errorMessage)
        {
            var body = new { message = errorMessage };
            return new APIGatewayProxyResponse
            {
                StatusCode = 404,
                Body       = JsonSerializer.Serialize(body),
                Headers    = DefaultHeaders
            };
        }

        /// <summary>
        /// Retorna 500 Internal Server Error com mensagem de erro.
        /// </summary>
        public static APIGatewayProxyResponse InternalError(string errorMessage)
        {
            var body = new { message = errorMessage };
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body       = JsonSerializer.Serialize(body),
                Headers    = DefaultHeaders
            };
        }
    }
}
