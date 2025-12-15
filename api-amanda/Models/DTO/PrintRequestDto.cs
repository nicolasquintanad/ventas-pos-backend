using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class PrintRequestDto
    {
        public int IdCaja { get; set; }
        public TipoImpresion Tipo { get; set; }

        // Contenido dinámico (Venta o Cierre)
        public JObject Data { get; set; }
    }
}