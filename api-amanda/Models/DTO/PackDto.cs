using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class PackDto
    {
        public string SKU_PACK { get; set; }
        public string NOMBRE_PACK { get; set; }
        public int PRECIO_PACK { get; set; }
        public bool EXCENTO_IVA { get; set; }

        // Lista de productos con cantidades
        public List<PackDetalleDto> Detalles { get; set; }
    }

    public class PackDetalleDto
    {
        public int ID_PRODUCTO { get; set; }
        public int CANTIDAD_PRODUCTO { get; set; }
    }
}