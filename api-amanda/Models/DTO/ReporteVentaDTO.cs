using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class ReporteVentaDTO
    {
        public DateTime FECHA { get; set; }
        public string Usuario { get; set; }
        public string Caja { get; set; }
        public string Producto { get; set; }
        public string Proveedor { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }
    }
}