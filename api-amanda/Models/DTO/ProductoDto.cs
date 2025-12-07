using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class ProductoDto
    {
        public string SKU { get; set; }
        public string NOMBRE { get; set; }
        public string DESCRIPCION { get; set; }
        public int PRECIO { get; set; }
        public decimal STOCK { get; set; }
        public bool EXCENTO_IVA { get; set; }
        public int? ID_TIPO_PRODUCTO { get; set; }
        public int? ID_ALERTA { get; set; }
        public int? ID_PROVEEDOR { get; set; }

    }
}