using System;

namespace FinSimAI.Models
{
    public class WorldState
    {
        public double Inflation { get; set; } = 3.1;
        public double InterestRates { get; set; } = 5.25;
        public double GlobalLiquidity { get; set; } = 100.0; // Índice base 100
        public string KeyEvents { get; set; } = "Equilibrio inicial de mercado.";

        public override string ToString()
        {
            string liquidityContext = GlobalLiquidity switch
            {
                < 30 => "SEQUÍA EXTREMA (Crisis de Liquidez)",
                < 60 => "CONTRACCIÓN MONETARIA (QT Agresivo)",
                < 90 => "LIGERAMENTE RESTRICTIVO",
                <= 110 => "NEUTRAL / EQUILIBRIO",
                < 140 => "EXPANSIÓN MODERADA (QE Leve)",
                < 170 => "EXPANSIÓN AGRESIVA (QE Masivo)",
                _ => "INUNDACIÓN DE LIQUIDEZ (Helicopter Money)"
            };
            return $"[WORLD STATE]: Inflación: {Inflation}%, Tasas de Interés: {InterestRates}%, " +
                   $"Liquidez Global: {GlobalLiquidity}/200 ({liquidityContext}), Eventos: {KeyEvents}";
        }
    }
}
