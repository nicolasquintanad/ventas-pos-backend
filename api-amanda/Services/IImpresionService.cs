using api_amanda.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_amanda.Services
{
    public interface IImpresionService
    {
        void ImprimirVenta(int idCaja, TicketVentaDto venta);
        void ImprimirCierre(int idCaja, TicketCierreDto cierre);
    }
}
