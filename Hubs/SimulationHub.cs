using Microsoft.AspNetCore.SignalR;
using FinSimAI.Models;
using FinSimAI.Services;

namespace FinSimAI.Hubs
{
    public class SimulationHub : Hub
    {
        // Estado compartido del motor (singleton en Program.cs)
        private static readonly List<Character> _characters = new List<Character>();
        private static readonly WorldState _worldState = new WorldState();
        private static OpenRouterService? _apiService;
        private static bool _isInitialized = false;

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _apiService = new OpenRouterService();
            InitializeCharacters();
            _isInitialized = true;
        }

        private void InitializeCharacters()
        {
            _characters.Clear();

            _characters.Add(new Character(
                "Vanguard-IG",
                "Eres un Banco Institucional (Vanguard/BlackRock). Representas el consenso de fondos de pensiones y soberanos.",
                "BALANCE SHEET: 50% Bonos Soberanos (G7), 30% Renta Variable Global (World Index), 15% Real Estate/Infraestructura, 5% Oro/Bitcoin (Reserva de Valor).",
                "MANDATO DE SUPERVIVENCIA: Tu prioridad es la PRESERVACIÓN DE PODER ADQUISITIVO REAL. Odias la volatilidad, PERO si el sistema fiduciario falla (Riesgo Sistémico/Impago), NO huyas al cash/bonos: REFÚGIATE en Activos Duros (Oro, BTC, Commodities).",
                "#00d4ff")); // Cyan

            _characters.Add(new Character(
                "Aura-Hedge",
                "Eres un Hedge Fund Global Macro. Representas el 'Smart Money' especulativo.",
                "BALANCE SHEET: 130% Gross Exposure (Long/Short). Long: Tech Disruptiva & Emerging Markets. Short: Zombie Companies & Divisas Débiles. 10% Volatility Hedges (VIX/Puts).",
                "MANDATO DE SUPERVIVENCIA: Buscas asimetría. Si el consenso se equivoca, apuestas fuerte en contra. Si hay pánico, monetiza tus coberturas y compra activos de calidad a precio de remate (Distressed Assets).",
                "#ff4d6a")); // Red

            _characters.Add(new Character(
                "Reddit-Retail",
                "Eres la masa de Inversores Minoristas Agregados. Representas el flujo de capital no profesional.",
                "BALANCE SHEET: 60% High Growth Tech (Nasdaq 100), 30% Criptoactivos (Majors & Alts), 10% Cash/Liquidez.",
                "MANDATO DE SUPERVIVENCIA: Momentum Trader. Si la tendencia es alcista, entras con todo (FOMO). Si la tendencia se rompe o hay miedo, vendes en masa (Panic Selling). No tienes cobertura, solo dirección.",
                "#ffd700")); // Yellow/Gold

            _characters.Add(new Character(
                "Citadel-MM",
                "Eres un Market Maker Institucional (Citadel/Virtu). Representas la infraestructura del mercado.",
                "BALANCE SHEET: Delta Neutral. Inventario masivo de todos los activos, cubierto con derivados.",
                "MANDATO DE SUPERVIVENCIA: Tu negocio es el volumen y el spread. Si la volatilidad es extrema, amplía los spreads para protegerte. Si detectas flujo tóxico (información asimétrica), retira la liquidez y deja que el precio colapse hasta encontrar equilibrio.",
                "#c850ff")); // Magenta

            AddToAllHistories("system", $"[ESTADO INICIAL DEL MERCADO]: {_worldState}", "SISTEMA");
        }

        // --- API Pública para el Frontend ---

        public async Task GetInitialState()
        {
            EnsureInitialized();
            await Clients.Caller.SendAsync("ReceiveWorldState", new
            {
                inflation = _worldState.Inflation,
                interestRates = _worldState.InterestRates,
                globalLiquidity = _worldState.GlobalLiquidity
            });

            var agentProfiles = _characters.Select(c => new
            {
                name = c.Name,
                color = c.HexColor,
                balanceSheet = c.BalanceSheet,
                mandate = c.Mandate,
                riskScore = c.RiskScore
            }).ToList();

            await Clients.Caller.SendAsync("ReceiveAgentProfiles", agentProfiles);

            // Enviar settings actuales (enmascaradas)
            await SendCurrentSettings();
        }

        public async Task SaveSettings(string provider, string openRouterKey, string openRouterModel,
            string openAIKey, string openAIModel, string googleKey, string googleModel,
            int maxTokens, double temperature)
        {
            try
            {
                string envPath = Path.Combine(AppContext.BaseDirectory, ".env");

                // Si una key viene enmascarada (contiene "..."), conservar la real del .env existente
                if (File.Exists(envPath))
                {
                    DotNetEnv.Env.Load(envPath);
                    if (IsMaskedKey(openRouterKey))
                        openRouterKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? "";
                    if (IsMaskedKey(openAIKey))
                        openAIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
                    if (IsMaskedKey(googleKey))
                        googleKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? "";
                }

                // Construir contenido del .env
                var envContent = $@"# Configuración de Proveedor de IA (openrouter | openai | google)
AI_PROVIDER={provider}

# --- OPENAI CONFIGURATION ---
OPENAI_API_KEY={openAIKey}
OPENAI_DEFAULT_CHAT_MODEL={openAIModel}

# --- OPENROUTER CONFIGURATION ---
OPENROUTER_API_KEY={openRouterKey}
DEFAULT_CHAT_MODEL={openRouterModel}

# --- GOOGLE GEMINI CONFIGURATION ---
GOOGLE_API_KEY={googleKey}
GOOGLE_DEFAULT_MODEL={googleModel}

# --- ENGINE PARAMETERS ---
MAX_TOKENS={maxTokens}
TEMPERATURE={temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)}
";

                await File.WriteAllTextAsync(envPath, envContent);

                // Recargar el servicio de API con las nuevas credenciales
                _apiService = new OpenRouterService();

                await Clients.Caller.SendAsync("SettingsSaved", new { success = true, message = "Configuración guardada y aplicada correctamente." });

                // Enviar settings actualizadas (enmascaradas)
                await SendCurrentSettings();
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("SettingsSaved", new { success = false, message = $"Error al guardar: {ex.Message}" });
            }
        }

        /// <summary>
        /// Detecta si una key fue enmascarada por el frontend (contiene "..." en el medio)
        /// </summary>
        private static bool IsMaskedKey(string key)
        {
            return !string.IsNullOrEmpty(key) && key.Contains("...");
        }

        public async Task GetSettings()
        {
            await SendCurrentSettings();
        }

        private async Task SendCurrentSettings()
        {
            // Cargar .env actual
            string envPath = Path.Combine(AppContext.BaseDirectory, ".env");
            if (!File.Exists(envPath))
            {
                await Clients.Caller.SendAsync("ReceiveSettings", new
                {
                    provider = "openrouter",
                    openRouterKey = "", openRouterModel = "openrouter/auto",
                    openAIKey = "", openAIModel = "gpt-4o",
                    googleKey = "", googleModel = "gemini-2.0-flash",
                    maxTokens = 800, temperature = 0.2,
                    hasEnvFile = false
                });
                return;
            }

            DotNetEnv.Env.Load(envPath);

            string maskKey(string? key)
            {
                if (string.IsNullOrEmpty(key) || key.Length < 8) return key ?? "";
                return key.Substring(0, 6) + "..." + key.Substring(key.Length - 4);
            }

            await Clients.Caller.SendAsync("ReceiveSettings", new
            {
                provider = Environment.GetEnvironmentVariable("AI_PROVIDER") ?? "openrouter",
                openRouterKey = maskKey(Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")),
                openRouterModel = Environment.GetEnvironmentVariable("DEFAULT_CHAT_MODEL") ?? "openrouter/auto",
                openAIKey = maskKey(Environment.GetEnvironmentVariable("OPENAI_API_KEY")),
                openAIModel = Environment.GetEnvironmentVariable("OPENAI_DEFAULT_CHAT_MODEL") ?? "gpt-4o",
                googleKey = maskKey(Environment.GetEnvironmentVariable("GOOGLE_API_KEY")),
                googleModel = Environment.GetEnvironmentVariable("GOOGLE_DEFAULT_MODEL") ?? "gemini-2.0-flash",
                maxTokens = int.TryParse(Environment.GetEnvironmentVariable("MAX_TOKENS"), out int mt) ? mt : 800,
                temperature = double.TryParse(Environment.GetEnvironmentVariable("TEMPERATURE"),
                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double temp) ? temp : 0.2,
                hasEnvFile = true
            });
        }

        public async Task UpdateWorldState(double inflation, double rates, double liquidity)
        {
            EnsureInitialized();
            _worldState.Inflation = inflation;
            _worldState.InterestRates = rates;
            _worldState.GlobalLiquidity = liquidity;

            await Clients.All.SendAsync("ReceiveWorldState", new
            {
                inflation = _worldState.Inflation,
                interestRates = _worldState.InterestRates,
                globalLiquidity = _worldState.GlobalLiquidity
            });
        }

        public async Task ResetSimulation()
        {
            // Reset WorldState a valores por defecto
            _worldState.Inflation = 3.1;
            _worldState.InterestRates = 5.25;
            _worldState.GlobalLiquidity = 100.0;
            _worldState.KeyEvents = "Equilibrio inicial de mercado.";

            // Limpiar historial de TODOS los agentes y re-inyectar solo el system prompt
            foreach (var character in _characters)
            {
                character.ChatHistory.Clear();
                character.ChatHistory.Add(new ChatMessage
                {
                    Role = "system",
                    Content = character.SystemPrompt
                });
                character.RiskScore = 0.0;
            }

            // Re-inyectar estado inicial del mercado
            AddToAllHistories("system", $"[ESTADO INICIAL DEL MERCADO]: {_worldState}", "SISTEMA");

            // Notificar al frontend
            await Clients.Caller.SendAsync("ReceiveWorldState", new
            {
                inflation = _worldState.Inflation,
                interestRates = _worldState.InterestRates,
                globalLiquidity = _worldState.GlobalLiquidity
            });

            await Clients.Caller.SendAsync("ReceiveMarketSentiment", new
            {
                riskIndex = 0.0,
                sentiment = "NEUTRAL / EQUILIBRIO"
            });

            await Clients.Caller.SendAsync("SimulationReset");
        }

        public async Task RunSimulation(string eventText)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(eventText)) return;

            // Parsear comandos macro (ej: inflation=5.0)
            if (eventText.Contains("="))
            {
                try
                {
                    var parts = eventText.Split('=');
                    string key = parts[0].Trim().ToLower();
                    double value = double.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    switch (key)
                    {
                        case "inflation": _worldState.Inflation = value; break;
                        case "rates": _worldState.InterestRates = value; break;
                        case "liquidity": _worldState.GlobalLiquidity = value; break;
                    }
                    await Clients.Caller.SendAsync("ReceiveWorldState", new
                    {
                        inflation = _worldState.Inflation,
                        interestRates = _worldState.InterestRates,
                        globalLiquidity = _worldState.GlobalLiquidity
                    });
                }
                catch { /* No es un comando macro, procesar como narrativa */ }
            }

            List<string> turnResponses = new List<string>();

            // --- SECUENCIA CAUSAL AISLADA ---

            // PASO A: RETAIL (Aislado)
            var retail = _characters.First(c => c.Name == "Reddit-Retail");
            AddToHistory(retail, "user", $"[SUCESO AMBIENTAL]: {eventText}");
            await Clients.Caller.SendAsync("AgentThinking", retail.Name);
            string retailResponse = await GetAgentResponse(retail);
            turnResponses.Add($"{retail.Name}: {retailResponse}");
            await SendAgentResponse(retail, retailResponse);

            // PASO B: HEDGE FUND (Aislado)
            var hedge = _characters.First(c => c.Name == "Aura-Hedge");
            AddToHistory(hedge, "user", $"[SUCESO AMBIENTAL]: {eventText}");
            await Clients.Caller.SendAsync("AgentThinking", hedge.Name);
            string hedgeResponse = await GetAgentResponse(hedge);
            turnResponses.Add($"{hedge.Name}: {hedgeResponse}");
            await SendAgentResponse(hedge, hedgeResponse);

            // PASO C: INSTITUCIONAL (Aislado)
            var inst = _characters.First(c => c.Name == "Vanguard-IG");
            AddToHistory(inst, "user", $"[SUCESO AMBIENTAL]: {eventText}");
            await Clients.Caller.SendAsync("AgentThinking", inst.Name);
            string instResponse = await GetAgentResponse(inst);
            turnResponses.Add($"{inst.Name}: {instResponse}");
            await SendAgentResponse(inst, instResponse);

            // PASO D: MARKET MAKER (Con Order Flow)
            string orderFlowSummary = GenerateOrderFlowSummary(retailResponse, hedgeResponse, instResponse);
            var mm = _characters.First(c => c.Name == "Citadel-MM");
            AddToHistory(mm, "user", $"[SUCESO AMBIENTAL]: {eventText}");
            await Clients.Caller.SendAsync("AgentThinking", mm.Name);
            string mmResponse = await GetAgentResponse(mm, orderFlowSummary);
            turnResponses.Add($"{mm.Name}: {mmResponse}");
            await SendAgentResponse(mm, mmResponse);

            // PASO E: CIO Agent
            await RunCIOAnalysis(eventText, turnResponses);

            // Sentimiento global
            double avgRisk = _characters.Average(c => c.RiskScore);
            await Clients.Caller.SendAsync("ReceiveMarketSentiment", new
            {
                riskIndex = Math.Round(avgRisk, 2),
                sentiment = avgRisk switch
                {
                    < -0.5 => "PÁNICO TOTAL",
                    < -0.1 => "MIEDO / BAJISTA",
                    <= 0.1 => "NEUTRAL / EQUILIBRIO",
                    < 0.5 => "OPTIMISMO / ALCISTA",
                    _ => "EUFORIA / BURBUJA"
                }
            });
        }

        // --- Métodos Privados ---

        private async Task<string> GetAgentResponse(Character character, string orderFlowInfo = "")
        {
            string dynamicPrompt = $"[MACRO STATE]: {_worldState}.\n" +
                                   $"[TU CARTERA ACTUAL]: {character.BalanceSheet}\n" +
                                   $"[TU MANDATO]: {character.Mandate}\n";

            if (character.Name == "Citadel-MM" && !string.IsNullOrEmpty(orderFlowInfo))
            {
                dynamicPrompt += $"\n[INFORMACIÓN PRIVILEGIADA / ORDER FLOW]: {orderFlowInfo}\n" +
                                 "Usa esto para validar si el movimiento es real o una trampa de liquidez.";
            }

            dynamicPrompt += "\nAl final de tu respuesta, finaliza OBLIGATORIAMENTE con:\n" +
                             "1. Tag [RISK: X.X] (-1.0 a 1.0).\n" +
                             "2. Tags [ORDER: BUY/SELL ASSET AMOUNT] (Uno por cada activo que muevas). Ej:\n" +
                             "[ORDER: SELL BONDS HIGH]\n" +
                             "[ORDER: BUY BTC MEDIUM]";

            var tempHistory = new List<ChatMessage>(character.ChatHistory);
            tempHistory.Add(new ChatMessage { Role = "system", Content = dynamicPrompt });

            string response = await _apiService!.GetChatResponseAsync(tempHistory);

            if (string.IsNullOrWhiteSpace(response))
                response = "[Analizando datos... sin respuesta concluyente]";

            // Extraer Risk Score
            try
            {
                int riskTagIndex = response.LastIndexOf("[RISK:");
                if (riskTagIndex != -1)
                {
                    string riskPart = response.Substring(riskTagIndex);
                    int colonIndex = riskPart.IndexOf(":");
                    int bracketIndex = riskPart.IndexOf("]");
                    if (colonIndex != -1 && bracketIndex != -1)
                    {
                        string scoreStr = riskPart.Substring(colonIndex + 1, bracketIndex - colonIndex - 1).Trim();
                        if (double.TryParse(scoreStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedRisk))
                        {
                            character.RiskScore = Math.Clamp(parsedRisk, -1.0, 1.0);
                        }
                    }
                }
            }
            catch { }

            AddToHistory(character, "assistant", response);
            return response;
        }

        private async Task SendAgentResponse(Character character, string response)
        {
            // Extraer todos los ORDER tags
            var orders = ExtractAllOrderTags(response);
            string? riskTag = ExtractRiskTag(response);

            await Clients.Caller.SendAsync("ReceiveAgentResponse", new
            {
                name = character.Name,
                color = character.HexColor,
                response = response,
                riskScore = character.RiskScore,
                orders = orders,
                riskTag = riskTag
            });
        }

        private async Task RunCIOAnalysis(string eventText, List<string> responses)
        {
            await Clients.Caller.SendAsync("CIOThinking");

            var cioHistory = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "system",
                    Content = "Eres el Analista Jefe (CIO). Tu tarea es sintetizar las reacciones de 4 entidades financieras (Vanguard, Aura-Hedge, Reddit, Citadel) ante un evento. " +
                              "Debes proporcionar: 1. Una síntesis narrativa de la situación. 2. Probabilidades (percentiles) de movimiento de mercado (Bullish/Neutral/Bearish). 3. Un JSON estructurado al final con campos: market_outlook, conviction_score, projections { short_term { bullish, neutral, bearish } }."
                }
            };

            string fullContext = $"Evento: {eventText}\n\nReacciones de agentes:\n" + string.Join("\n\n", responses);
            cioHistory.Add(new ChatMessage { Role = "user", Content = fullContext });

            string cioOutput = await _apiService!.GetChatResponseAsync(cioHistory);

            // Extraer JSON del CIO output
            string? jsonBlock = ExtractJsonBlock(cioOutput);

            await Clients.Caller.SendAsync("ReceiveCIOReport", new
            {
                narrative = cioOutput,
                jsonData = jsonBlock
            });

            // Sincronizar memoria de todos los agentes
            AddToAllHistories("user", $"[REPORTE OFICIAL DEL DÍA - CIO]: {cioOutput}", "SISTEMA");
        }

        // --- Utilidades ---

        private string GenerateOrderFlowSummary(string retail, string hedge, string inst)
        {
            string retailOrder = ExtractOrderTag(retail) ?? "NO CLEAR ORDER (Retail confusion)";
            string hedgeOrder = ExtractOrderTag(hedge) ?? "HEDGING / UNCERTAIN";
            string instOrder = ExtractOrderTag(inst) ?? "HOLDING CASH (Risk-Off)";

            return $"[ORDER FLOW REPORT / REAL-TIME DATA]:\n" +
                   $"1. RETAIL ACTIVITY: {retailOrder}\n" +
                   $"2. HEDGE FUND POSITIONING: {hedgeOrder}\n" +
                   $"3. INSTITUTIONAL FLOW: {instOrder}\n" +
                   $"ANÁLISIS DE PROFUNDIDAD: Compara el volumen Retail (ruido) con el Flujo Institucional (tendencia real). " +
                   $"Si Institucional y Retail van en la misma dirección, hay LIQUIDEZ. Si van opuestos, es una TRAMPA DE LIQUIDEZ.";
        }

        private string? ExtractOrderTag(string text)
        {
            var matches = ExtractAllOrderTags(text);
            return matches.Length > 0 ? string.Join(", ", matches) : null;
        }

        private string[] ExtractAllOrderTags(string text)
        {
            var matches = new List<string>();
            int index = 0;
            while (true)
            {
                index = text.IndexOf("[ORDER:", index);
                if (index == -1) break;
                int endIndex = text.IndexOf("]", index);
                if (endIndex == -1) break;
                matches.Add(text.Substring(index, endIndex - index + 1));
                index = endIndex + 1;
            }
            return matches.ToArray();
        }

        private string? ExtractRiskTag(string text)
        {
            int index = text.LastIndexOf("[RISK:");
            if (index == -1) return null;
            int endIndex = text.IndexOf("]", index);
            if (endIndex == -1) return null;
            return text.Substring(index, endIndex - index + 1);
        }

        private string? ExtractJsonBlock(string text)
        {
            int start = text.IndexOf("```json");
            if (start == -1) start = text.IndexOf("{");
            else start = text.IndexOf("{", start);

            if (start == -1) return null;

            int depth = 0;
            for (int i = start; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                if (text[i] == '}') depth--;
                if (depth == 0) return text.Substring(start, i - start + 1);
            }
            return null;
        }

        private void AddToHistory(Character character, string role, string content)
        {
            character.ChatHistory.Add(new ChatMessage { Role = role, Content = content });
            if (character.ChatHistory.Count > 15) character.ChatHistory.RemoveAt(1);
        }

        private void AddToAllHistories(string role, string content, string speakerName = "Tú")
        {
            foreach (var character in _characters)
            {
                string prefix = (speakerName == character.Name) ? "" : $"{speakerName} dice: ";
                AddToHistory(character, role, prefix + content);
            }
        }
    }
}
