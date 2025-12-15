using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class ImpresoraDto
    {
        public int IdImpresora { get; set; }
        public string NombreWindows { get; set; }
        public string Tipo { get; set; }
        public bool Activa { get; set; }
        public int? IdCaja { get; set; } // asociación opcional
        public string NombreCaja { get; set; }
    }
}