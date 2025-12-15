using api_amanda.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace api_amanda.Formatters
{
    public static class TicketVentaFormatter
    {
        // 80mm suele verse bien con 48 chars; si prefieres más compacto, usa 32
        private const int LineWidth = 48;

        public static string Formatear(TicketVentaDto t)
        {
            var sb = new StringBuilder();

            // --- ENCABEZADO ---
            sb.AppendLine(Center(t.Empresa ?? "AMANDA MINIMARKET Y BOTILLERIA"));
            if (!string.IsNullOrWhiteSpace(t.Rut))
                sb.AppendLine(Center($"RUT: {t.Rut}"));
            if (!string.IsNullOrWhiteSpace(t.Direccion))
                sb.AppendLine(Center(t.Direccion));

            sb.AppendLine(new string('-', LineWidth));

            // --- INFO VENTA ---
            sb.AppendLine($"Caja: {t.Caja}");
            sb.AppendLine($"Fecha: {t.Fecha:dd/MM/yyyy HH:mm}");
            sb.AppendLine(new string('-', LineWidth));

            // --- CABECERA ITEMS ---
            sb.AppendLine(Cols("Cant", "Producto", "Subtotal"));
            sb.AppendLine(new string('-', LineWidth));

            // --- ITEMS ---
            foreach (var i in t.Items ?? Enumerable.Empty<TicketVentaItemDto>())
            {
                // Nombre puede ir en 2 líneas si es largo
                var nombre = i.Nombre ?? "";
                var subtotal = FmtPeso(i.Subtotal);

                // Primera línea: cantidad + nombre truncado
                sb.AppendLine(Cols(
                    i.Cantidad.ToString(),
                    Trunc(nombre, 28),
                    subtotal
                ));

                // Si el nombre es más largo, continuamos debajo
                var resto = nombre.Length > 28 ? nombre.Substring(28) : "";
                while (!string.IsNullOrEmpty(resto))
                {
                    sb.AppendLine(Cols(
                        "",
                        Trunc(resto, 28),
                        ""
                    ));
                    resto = resto.Length > 28 ? resto.Substring(28) : "";
                }
            }

            sb.AppendLine(new string('-', LineWidth));

            // --- TOTAL (DESTACADO) ---
            sb.Append(BoldOn());
            sb.AppendLine(Right($"TOTAL: {FmtPeso(t.Total)}"));
            sb.Append(BoldOff());

            sb.AppendLine(new string('-', LineWidth));
            sb.AppendLine(Center("¡Gracias por su compra!"));
            sb.AppendLine("\n");

            // --- CORTE ---
            sb.Append(Cut());

            return sb.ToString();
        }

        // ===== Helpers =====

        private static string FmtPeso(decimal v)
            => "$" + string.Format("{0:N0}", v);

        private static string Trunc(string s, int len)
            => string.IsNullOrEmpty(s) ? "" : (s.Length <= len ? s : s.Substring(0, len));

        // Columnas: Cant(4) | Producto(28) | Subtotal(12) = 44 (+ espacios)
        private static string Cols(string c1, string c2, string c3)
        {
            c1 = (c1 ?? "").PadRight(4);
            c2 = (c2 ?? "").PadRight(28);
            c3 = (c3 ?? "").PadLeft(12);
            return $"{c1} {c2} {c3}";
        }

        private static string Center(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length >= LineWidth) return text;
            int left = (LineWidth - text.Length) / 2;
            return new string(' ', left) + text;
        }

        private static string Right(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length >= LineWidth) return text;
            return new string(' ', LineWidth - text.Length) + text;
        }

        // --- ESC/POS ---
        private static string BoldOn() => $"{(char)27}E{(char)1}";
        private static string BoldOff() => $"{(char)27}E{(char)0}";

        // Corte total: GS V B n  -> 29 86 66 0
        private static string Cut() => $"{(char)29}V{(char)66}{(char)0}";
    }
}