using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class EntradaStockDto
    {
        public int ID_PRODUCTO { get; set; }
        public int ID_PROVEEDOR { get; set; }
        public decimal CANTIDAD { get; set; } // DECIMAL(18,6)
        public decimal COSTO_UNITARIO { get; set; } // DECIMAL(10,2)
        public int ID_USUARIO { get; set; }   // Usuario logueado
    }
}