using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class TicketVentaDto
    {
        public string Empresa { get; set; }
        public string Rut { get; set; }
        public string Direccion { get; set; }

        public string Caja { get; set; }
        public DateTime Fecha { get; set; }

        public List<TicketVentaItemDto> Items { get; set; }

        public decimal Total { get; set; }
    }

    public class TicketVentaItemDto
    {
        public int Cantidad { get; set; }
        public string Nombre { get; set; }
        public decimal Subtotal { get; set; }
    }
}