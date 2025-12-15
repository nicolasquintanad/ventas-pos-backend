using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class TicketDto
    {
        public string Empresa { get; set; }
        public string Direccion { get; set; }
        public string Caja { get; set; }
        public DateTime Fecha { get; set; }
        public List<TicketItemDto> Items { get; set; }
        public decimal Total { get; set; }
    }

    public class TicketItemDto
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
    }
}