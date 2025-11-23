using api_amanda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace api_amanda.Controllers
{
    [RoutePrefix("products/types")]
    public class TipoProductoController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetTipos()
        {
            using (var db = new AMANDAEntities())
            {
                var tipos = db.TIPO_PRODUCTO.Select(t => new
                {
                    id = t.ID_TIPO_PRODUCTO,
                    name = t.NOMBRE,
                    cigarrillo = t.CIGARRILLO
                }).ToList();

                return Ok(tipos);
            }
        }
    }
}
