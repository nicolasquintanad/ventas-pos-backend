using System.Linq;
using System.Web.Http;
using api_amanda.Models;

namespace api_amanda.Controllers
{
    [RoutePrefix("proveedores")]
    public class ProveedorController : ApiController
    {
        // GET /proveedores
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetProveedores()
        {
            using (var db = new AMANDAEntities())
            {
                var proveedores = db.PROVEEDOR
                    .OrderBy(p => p.NOMBRE)
                    .Select(p => new
                    {
                        id = p.ID_PROVEEDOR,
                        nombre = p.NOMBRE
                    })
                    .ToList();

                return Ok(proveedores);
            }
        }
    }
}
