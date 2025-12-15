using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using api_amanda.Models.DTO;
using api_amanda.Helpers;
using api_amanda.Models;
using api_amanda.Formatters;

namespace api_amanda.Services
{
    public class ImpresionService : IImpresionService
    {
        // 🔹 MÉTODO INTERNO: OBTENER IMPRESORA SEGÚN CAJA
        private string ObtenerNombreImpresoraPorCaja(int idCaja)
        {
            using (var db = new AMANDAEntities())
            {
                return (
                    from ci in db.CAJA_IMPRESORA
                    join i in db.IMPRESORA on ci.ID_IMPRESORA equals i.ID_IMPRESORA
                    where ci.ID_CAJA == idCaja && i.ACTIVA
                    select i.NOMBRE_WINDOWS
                ).FirstOrDefault();
            }
        }

        // 🔹 EJEMPLO: IMPRESIÓN DE VENTA
        public void ImprimirVenta(int idCaja, TicketVentaDto venta)
        {
            var impresora = ObtenerNombreImpresoraPorCaja(idCaja);

            if (string.IsNullOrEmpty(impresora))
                throw new Exception("La caja no tiene impresora asociada");

            var texto = TicketVentaFormatter.Formatear(venta);

            RawPrinterHelper.SendStringToPrinter(impresora, texto);
        }

        // 🔹 EJEMPLO: IMPRESIÓN DE CIERRE
        public void ImprimirCierre(int idCaja, TicketCierreDto cierre)
        {
            var impresora = ObtenerNombreImpresoraPorCaja(idCaja);

            if (string.IsNullOrEmpty(impresora))
                throw new Exception("La caja no tiene impresora asociada");

            var texto = TicketCierreFormatter.Formatear(cierre);

            RawPrinterHelper.SendStringToPrinter(impresora, texto);
        }
    }
}