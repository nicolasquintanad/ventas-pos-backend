using api_amanda.Helpers;
using api_amanda.Models.DTO;
using api_amanda.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace api_amanda.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/impresion")]
    public class ImpresionController : ApiController
    {
        private readonly IImpresionService _impresion;

        public ImpresionController(IImpresionService impresion)
        {
            _impresion = impresion;
        }

        [HttpPost]
        [Route("print")]
        public IHttpActionResult Print(PrintRequestDto req)
        {
            if (req == null)
                return BadRequest("Request vacío");

            if (req.IdCaja <= 0)
                return BadRequest("IdCaja inválido");

            switch (req.Tipo)
            {
                case TipoImpresion.Venta:
                    {
                        var venta = req.Data.ToObject<TicketVentaDto>();
                        _impresion.ImprimirVenta(req.IdCaja, venta);
                        break;
                    }

                case TipoImpresion.Cierre:
                    {
                        var cierre = req.Data.ToObject<TicketCierreDto>();
                        _impresion.ImprimirCierre(req.IdCaja, cierre);
                        break;
                    }

                default:
                    return BadRequest("Tipo de impresión no soportado");
            }

            return Ok();
        }
    }
}
