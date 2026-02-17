using System;
using System.Collections.Generic;

namespace FinSimAI.Models
{
    /// <summary>
    /// Representa un agente financiero en la simulación.
    /// </summary>
    public class Character
    {
        public string Name { get; set; }
        public string SystemPrompt { get; set; }
        public string BalanceSheet { get; set; }
        public string Mandate { get; set; }
        public string HexColor { get; set; }          // Color CSS (ej: "#00d4ff")
        public ConsoleColor Color { get; set; }         // Color de consola (legacy)

        // Cuantifica el posicionamiento de riesgo (-1.0: Máximo Miedo, 1.0: Máxima Codicia)
        public double RiskScore { get; set; } = 0.0;

        // Historial de mensajes para este personaje específico
        public List<ChatMessage> ChatHistory { get; set; }

        // Constructor actualizado para UI web
        public Character(string name, string systemPrompt, string balanceSheet, string mandate, string hexColor)
        {
            Name = name;
            SystemPrompt = systemPrompt;
            BalanceSheet = balanceSheet;
            Mandate = mandate;
            HexColor = hexColor;
            Color = ConsoleColor.White; // fallback
            ChatHistory = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = systemPrompt }
            };
        }

        // Constructor legacy (consola)
        public Character(string name, string systemPrompt, string balanceSheet, string mandate, ConsoleColor color)
        {
            Name = name;
            SystemPrompt = systemPrompt;
            BalanceSheet = balanceSheet;
            Mandate = mandate;
            Color = color;
            HexColor = "#ffffff";
            ChatHistory = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = systemPrompt }
            };
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
