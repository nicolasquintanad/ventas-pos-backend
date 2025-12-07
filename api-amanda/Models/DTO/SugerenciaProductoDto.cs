using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class SugerenciaProductoDto
    {
        public int ID_PRODUCTO { get; set; }
        public string SKU { get; set; }
        public string NOMBRE { get; set; }
        public decimal STOCK { get; set; }

        public decimal VendidoPeriodo { get; set; }
        public int DiasPeriodo { get; set; }
        public decimal PromedioDiario { get; set; }
        public decimal DemandaEsperadaHorizonte { get; set; }
        public decimal CantidadSugerida { get; set; }

        public int StockMinimo { get; set; }
    }
}