using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class VentaItemDto
    {
        // "producto" o "pack"
        public string Tipo { get; set; }
        public int Id { get; set; }          // ID_PRODUCTO o ID_PACK
        public string Sku { get; set; }
        public decimal Cantidad { get; set; }    // por ahora entero (tabla DETALLE_VENTA usa int)
        public decimal PrecioUnitario { get; set; }
        public bool ExcentoIva { get; set; }
    }

    public class VentaDto
    {
        public int IdUsuario { get; set; }          // USUARIO que vende
        public int? IdCaja { get; set; }            // opcional, para integrar con CAJA después
        public int DescuentoAplicado { get; set; }  // 0 si no hay
        public List<VentaItemDto> Items { get; set; }
    }
}