using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class TipoProductoDto
    {
        public int ID { get; set; }
        public string NOMBRE { get; set; }
        public bool CIGARRILLO { get; set; }
    }
}