using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class ProveedorDto
    {
        public int ID_PROVEEDOR { get; set; }
        public string NOMBRE { get; set; }
        public string TELEFONO { get; set; }
        public string CORREO { get; set; }
        public string DIRECCION { get; set; }
        public string CIUDAD { get; set; }
        public bool VISITA_LUNES { get; set; }
        public bool VISITA_MARTES { get; set; }
        public bool VISITA_MIERCOLES { get; set; }
        public bool VISITA_JUEVES { get; set; }
        public bool VISITA_VIERNES { get; set; }
        public bool VISITA_SABADO { get; set; }
        public bool VISITA_DOMINGO { get; set; }
    }
}