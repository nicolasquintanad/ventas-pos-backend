using api_amanda.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace api_amanda.Formatters
{
    public static class TicketCierreFormatter
    {
        // Ajusta el ancho según tu ticket (80mm suele ser 48 chars aprox, 58mm 32 chars)
        private const int LineWidth = 32;

        public static string Formatear(TicketCierreDto data)
        {
            var sb = new StringBuilder();

            // --- TÍTULO ---
            sb.AppendLine(Center("*** CIERRE DE CAJA ***"));
            sb.AppendLine(Center($"Cierre #{data.IdAperturaCierre}"));
            sb.AppendLine(new string('-', LineWidth));

            // --- INFO GENERAL ---
            sb.AppendLine($"Caja: {data.Caja}");
            sb.AppendLine($"Usuario: {data.Usuario}");
            sb.AppendLine($"Apertura: {FmtFecha(data.FechaApertura)}");
            sb.AppendLine($"Cierre:   {FmtFecha(data.FechaCierre)}");
            sb.AppendLine(new string('-', LineWidth));

            // --- MONTOS ---
            sb.AppendLine($"Monto inicial:     {FmtPeso(data.MontoInicial)}");
            sb.AppendLine($"Total productos:   {FmtPeso(data.MontoProductos)}");
            sb.AppendLine($"Total cigarrillos: {FmtPeso(data.MontoCigarros)}");

            sb.AppendLine(new string('-', LineWidth));

            sb.AppendLine($"Ventas totales:    {FmtPeso(data.TotalVentas)}");
            sb.AppendLine(new string('=', LineWidth));
            sb.AppendLine(Center($"MONTO FINAL: {FmtPeso(data.MontoFinal)}"));
            sb.AppendLine(new string('=', LineWidth));

            // --- RESUMEN ---
            sb.AppendLine($"Transacciones: {data.TotalTransacciones}");
            sb.AppendLine($"Prod. vendidos: {data.ProductosVendidos}");
            sb.AppendLine($"Packs vendidos: {data.PacksVendidos}");
            sb.AppendLine($"Cigarros vend.: {(data.ContieneCigarros ? "Sí" : "No")}");
            sb.AppendLine(new string('-', LineWidth));

            sb.AppendLine(Center("Gracias"));
            sb.AppendLine("\n\n");

            // --- CORTE (ESC/POS) ---
            sb.Append(Cut());

            return sb.ToString();
        }

        private static string FmtFecha(DateTime dt)
            => dt.ToString("dd/MM/yyyy HH:mm");

        private static string FmtPeso(decimal v)
            => "$" + string.Format("{0:N0}", v);

        private static string Center(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length >= LineWidth) return text;

            int left = (LineWidth - text.Length) / 2;
            return new string(' ', left) + text;
        }

        // Corte total: GS V B n  -> 29 86 66 0
        private static string Cut()
            => $"{(char)29}V{(char)66}{(char)0}";
    }
}