using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api_amanda.Models.DTO
{
    public class TicketCierreDto
    {
        public int IdAperturaCierre { get; set; }

        public string Caja { get; set; }
        public string Usuario { get; set; }

        public DateTime FechaApertura { get; set; }
        public DateTime FechaCierre { get; set; }

        public decimal MontoInicial { get; set; }
        public decimal MontoProductos { get; set; }
        public decimal MontoCigarros { get; set; }

        public decimal TotalVentas { get; set; }
        public decimal MontoFinal { get; set; }

        public int TotalTransacciones { get; set; }
        public int ProductosVendidos { get; set; }
        public int PacksVendidos { get; set; }

        public bool ContieneCigarros { get; set; }
    }
}