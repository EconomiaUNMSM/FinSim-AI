using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinSimAI.Models;
using FinSimAI.Services;

namespace FinSimAI.Core
{
    public class GameEngine
    {
        private readonly List<Character> _characters;
        private readonly OpenRouterService _apiService;
        private readonly Random _random = new Random();
        private const int MaxTokensPerResponse = 800; // Sincronizado con el servicio
        private readonly WorldState _worldState = new WorldState();

        public GameEngine()
        {
            _apiService = new OpenRouterService();
            _characters = new List<Character>();
            InitializeCharacters();
        }

        private void InitializeCharacters()
        {
            // 1. VANGUARD (Institucional) - Carteras Modernas (60/40 + Alt)
            _characters.Add(new Character(
                "Vanguard-IG", 
                "Eres un Banco Institucional (Vanguard/BlackRock). Representas el consenso de fondos de pensiones y soberanos.",
                "BALANCE SHEET: 50% Bonos Soberanos (G7), 30% Renta Variable Global (World Index), 15% Real Estate/Infraestructura, 5% Oro/Bitcoin (Reserva de Valor).",
                "MANDATO DE SUPERVIVENCIA: Tu prioridad es la PRESERVACIÓN DE PODER ADQUISITIVO REAL. Odias la volatilidad, PERO si el sistema fiduciario falla (Riesgo Sistémico/Impago), NO huyas al cash/bonos: REFÚGIATE en Activos Duros (Oro, BTC, Commodities).",
                ConsoleColor.Cyan));

            // 2. HEDGE FUND (Especulador) - Global Macro
            _characters.Add(new Character(
                "Aura-Hedge", 
                "Eres un Hedge Fund Global Macro. Representas el 'Smart Money' especulativo.",
                "BALANCE SHEET: 130% Gross Exposure (Long/Short). Long: Tech Disruptiva & Emerging Markets. Short: Zombie Companies & Divisas Débiles. 10% Volatility Hedges (VIX/Puts).",
                "MANDATO DE SUPERVIVENCIA: Buscas asimetría. Si el consenso se equivoca, apuestas fuerte en contra. Si hay pánico, monetiza tus coberturas y compra activos de calidad a precio de remate (Distressed Assets).",
                ConsoleColor.Red));

            // 3. RETAIL (La Masa) - Sentimiento Agregado
            _characters.Add(new Character(
                "Reddit-Retail", 
                "Eres la masa de Inversores Minoristas Agregados. Representas el flujo de capital no profesional.",
                "BALANCE SHEET: 60% High Growth Tech (Nasdaq 100), 30% Criptoactivos (Majors & Alts), 10% Cash/Liquidez.",
                "MANDATO DE SUPERVIVENCIA: Momentum Trader. Si la tendencia es alcista, entras con todo (FOMO). Si la tendencia se rompe o hay miedo, vendes en masa (Panic Selling). No tienes cobertura, solo dirección.",
                ConsoleColor.Yellow));

            // 4. MARKET MAKER (El Árbitro) - Proveedor de Liquidez
            _characters.Add(new Character(
                "Citadel-MM", 
                "Eres un Market Maker Institucional (Citadel/Virtu). Representas la infraestructura del mercado.",
                "BALANCE SHEET: Delta Neutral. Inventario masivo de todos los activos, cubierto con derivados.",
                "MANDATO DE SUPERVIVENCIA: Tu negocio es el volumen y el spread. Si la volatilidad es extrema, amplía los spreads para protegerte. Si detectas flujo tóxico (información asimétrica), retira la liquidez y deja que el precio colapse hasta encontrar equilibrio.",
                ConsoleColor.Magenta));
        }

        public async Task StartAsync()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║            FINSIM-AI: SIMULADOR DE MERCADO               ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine($"\n[CONTEXTO INICIAL]: {_worldState}");
            Console.WriteLine("Eres el Arquitecto Macro (FED, Presidente o Eventos Mundiales).");
            Console.WriteLine("Las entidades reaccionarán a tus decisiones y entre sí.\n");

            // Inyectamos el mismo contexto centralizado en la memoria de los LLMs
            AddToAllHistories("system", $"[ESTADO INICIAL DEL MERCADO]: {_worldState}", "SISTEMA");

            while (true)
            {
                DisplayMarketSentiment();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\n[FED/Presidente/Evento]: ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.ToLower() == "salir") break;

                // Intento de parseo de comandos macro (ej: inflation=5.0)
                if (input.Contains("="))
                {
                    try
                    {
                        var parts = input.Split('=');
                        string key = parts[0].Trim().ToLower();
                        double value = double.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                        switch (key)
                        {
                            case "inflation": _worldState.Inflation = value; break;
                            case "rates": _worldState.InterestRates = value; break;
                            case "liquidity": _worldState.GlobalLiquidity = value; break;
                        }
                        Console.WriteLine($"\n[SISTEMA]: {_worldState}");
                    } catch { /* No es un comando válido, procesar como narrativa */ }
                }

                // 2. SECUENCIA CAUSAL AISLADA (Paralelismo Cognitivo)
                // Retail, Hedge e Inst NO deben saber qué piensan los otros.
                // Solo reaccionan al Evento del Usuario.

                List<string> turnResponses = new List<string>();
                
                // PASO A: RETAIL (Aislado)
                var retail = _characters.First(c => c.Name == "Reddit-Retail");
                AddToHistory(retail, "user", $"[SUCESO AMBIENTAL]: {input}"); // Solo él lo sabe
                await Task.Delay(2000);
                string retailResponse = await CharacterResponseAsync(retail);
                turnResponses.Add($"{retail.Name}: {retailResponse}");

                // PASO B: HEDGE FUND (Aislado)
                var hedge = _characters.First(c => c.Name == "Aura-Hedge");
                AddToHistory(hedge, "user", $"[SUCESO AMBIENTAL]: {input}"); // Solo él lo sabe
                await Task.Delay(2000);
                string hedgeResponse = await CharacterResponseAsync(hedge);
                turnResponses.Add($"{hedge.Name}: {hedgeResponse}");

                // PASO C: INSTITUCIONAL (Aislado)
                var inst = _characters.First(c => c.Name == "Vanguard-IG");
                AddToHistory(inst, "user", $"[SUCESO AMBIENTAL]: {input}"); // Solo él lo sabe
                await Task.Delay(2000);
                string instResponse = await CharacterResponseAsync(inst);
                turnResponses.Add($"{inst.Name}: {instResponse}");

                // PASO D: MARKET MAKER (Cierre / El que ve todo)
                // Generamos el "Order Flow" basado en los 3 anteriores (que ellos no vieron entre sí)
                string orderFlowSummary = GenerateOrderFlowSummary(retailResponse, hedgeResponse, instResponse);
                
                var mm = _characters.First(c => c.Name == "Citadel-MM");
                AddToHistory(mm, "user", $"[SUCESO AMBIENTAL]: {input}"); // Él también ve el evento
                await Task.Delay(2000);
                // Le pasamos la info privilegiada de lo que hicieron los otros
                string mmResponse = await CharacterResponseAsync(mm, orderFlowSummary);
                turnResponses.Add($"{mm.Name}: {mmResponse}");

                // 3. El 5to Agente (CIO) sintetiza y proyecta
                await CallCIOAgentAsync(input, turnResponses);
            }
        }

        private async Task CallCIOAgentAsync(string eventText, List<string> responses)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("║            ANALISTA CIO: PROYECCIÓN ESTRATÉGICA          ║");
            Console.WriteLine(new string('=', 60));

            var cioHistory = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = "Eres el Analista Jefe (CIO). Tu tarea es sintetizar las reacciones de 4 entidades financieras (Vanguard, Aura-Hedge, Reddit, Citadel) ante un evento. " +
                    "Debes proporcionar: 1. Una síntesis narrativa de la situación. 2. Probabilidades (percentiles) de movimiento de mercado (Bullish/Neutral/Bearish). 3. Un JSON estructurado al final con campos: market_outlook, conviction_score, projections { short_term { bullish, neutral, bearish } }." }
            };

            string fullContext = $"Evento: {eventText}\n\nReacciones de agentes:\n" + string.Join("\n\n", responses);
            cioHistory.Add(new ChatMessage { Role = "user", Content = fullContext });

            Console.WriteLine("\n[CIO procesando síntesis macro...]");
            string cioOutput = await _apiService.GetChatResponseAsync(cioHistory);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{cioOutput}");
            Console.ResetColor();

            // Sincronización de Memoria Global:
            // Todos los agentes "leen" el reporte del CIO y aprenden qué pasó en el mercado hoy.
            AddToAllHistories("user", $"[REPORTE OFICIAL DEL DÍA - CIO]: {cioOutput}", "SISTEMA");
        }

        private string GenerateOrderFlowSummary(string retail, string hedge, string inst)
        {
            // Extraemos las órdenes reales de los mensajes anteriores usando los tags estandarizados
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
            var matches = new List<string>();
            int index = 0;
            while (true)
            {
                index = text.IndexOf("[ORDER:", index);
                if (index == -1) break;

                int endIndex = text.IndexOf("]", index);
                if (endIndex == -1) break;

                string order = text.Substring(index, endIndex - index + 1);
                matches.Add(order);
                index = endIndex + 1;
            }

            return matches.Count > 0 ? string.Join(", ", matches) : null;
        }

        private void DisplayMarketSentiment()
        {
            double avgRisk = _characters.Average(c => c.RiskScore);
            string sentiment = avgRisk switch
            {
                < -0.5 => "PÁNICO TOTAL",
                < -0.1 => "MIEDO / BAJISTA",
                <= 0.1 => "NEUTRAL / EQUILIBRIO",
                < 0.5 => "OPTIMISMO / ALCISTA",
                _ => "EUFORIA / BURBUJA"
            };

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n[SENTIMIENTO DE MERCADO: {sentiment} | Risk Index: {avgRisk:F2}]");
            Console.ResetColor();
        }

        private async Task<string> CharacterResponseAsync(Character character, string orderFlowInfo = "")
        {
            Console.ForegroundColor = character.Color;
            Console.Write($"{character.Name}: ");
            
            // Construimos el System Prompt Dinámico
            string dynamicPrompt = $"[MACRO STATE]: {_worldState}.\n" +
                                   $"[TU CARTERA ACTUAL]: {character.BalanceSheet}\n" +
                                   $"[TU MANDATO]: {character.Mandate}\n";

            // Si es Citadel (Market Maker), le damos la info privilegiada
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

            string response = await _apiService.GetChatResponseAsync(tempHistory);
            
            if (string.IsNullOrWhiteSpace(response))
            {
                response = "[Analizando datos... sin respuesta concluyente]";
            }

            // Extracción de RiskScore (Mantenemos la lógica existente)
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
            } catch { }

            Console.WriteLine(response);

            // IMPORTANTE: Un agente recuerda su propia respuesta como "assistant"
            AddToHistory(character, "assistant", response);
            return response;
        }

        private void AddToHistory(Character character, string role, string content)
        {
             character.ChatHistory.Add(new ChatMessage 
             { 
                 Role = role, 
                 Content = content 
             });

             // Limitamos el historial para no exceder tokens
             if (character.ChatHistory.Count > 15) character.ChatHistory.RemoveAt(1);
        }

        private void AddToAllHistories(string role, string content, string speakerName = "Tú")
        {
            // Este método ahora se usará SOLO para inyectar el Resultado Final (CIO) 
            // a todos los agentes para que tengan memoria común del pasado.
            foreach (var character in _characters)
            {
                string prefix = (speakerName == character.Name) ? "" : $"{speakerName} dice: ";
                AddToHistory(character, role, prefix + content);
            }
        }
    }
}
