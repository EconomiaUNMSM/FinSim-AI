using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FinSimAI.Models;
using DotNetEnv;

namespace FinSimAI.Services
{
    /// <summary>
    /// Servicio para comunicarse con la API de OpenRouter.
    /// </summary>
    public class OpenRouterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _baseUrl;
        private readonly string _provider;

        public OpenRouterService()
        {
            // Cargamos las variables del archivo .env desde la misma ruta que SaveSettings usa
            string envPath = Path.Combine(AppContext.BaseDirectory, ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
            }
            else
            {
                // Fallback: intentar cargar desde el directorio de trabajo (útil en desarrollo)
                string fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
                if (File.Exists(fallbackPath))
                    Env.Load(fallbackPath);
            }
            
            // Determinamos el proveedor (por defecto OpenRouter si no se especifica)
            _provider = Environment.GetEnvironmentVariable("AI_PROVIDER")?.ToLower() ?? "openrouter";

            if (_provider == "openai")
            {
                _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
                _model = Environment.GetEnvironmentVariable("OPENAI_DEFAULT_CHAT_MODEL") ?? "gpt-4-turbo";
                _baseUrl = "https://api.openai.com/v1/chat/completions";
            }
            else
            {
                _apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? "";
                _model = Environment.GetEnvironmentVariable("DEFAULT_CHAT_MODEL") ?? "openrouter/auto";
                _baseUrl = "https://openrouter.ai/api/v1/chat/completions";
            }

            _httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
            
            // Headers específicos de OpenRouter (solo si usamos ese proveedor)
            if (_provider == "openrouter")
            {
                _httpClient.DefaultRequestHeaders.Add("X-Title", "FinSim-AI Market Simulator");
                // _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost"); // Opcional
            }
        }

        public async Task<string> GetChatResponseAsync(List<ChatMessage> messages)
        {
            var requestBody = new
            {
                model = _model,
                messages = messages,
                max_tokens = 800, // Aumentado para evitar cortes en análisis complejos
                temperature = 0.2 // Baja temperatura para mayor determinismo y coherencia lógica
            };

            string jsonRequest = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });

            try
            {
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_baseUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return $"[Error de API: {response.StatusCode} - {error}]";
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                
                // LOG DE DEPURACIÓN (Opcional, puedes comentarlo luego)
                // Console.WriteLine($"\n[LOG API]: {jsonResponse}");

                using var doc = JsonDocument.Parse(jsonResponse);
                
                if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var contentElement))
                    {
                        return contentElement.GetString() ?? "[Error: Contenido nulo]";
                    }
                }
                
                if (doc.RootElement.TryGetProperty("error", out var apiError))
                {
                    return $"[Detalle de Error: {apiError.GetRawText()}]";
                }

                return "[Error: Estructura de respuesta inesperada o vacía]";
            }
            catch (Exception ex)
            {
                return $"[Excepción de Red/JSON: {ex.Message}]";
            }
        }
    }
}
