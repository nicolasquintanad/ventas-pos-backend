using System;
using System.Linq;
using System.Web.Http;
using api_amanda.Models;
using api_amanda.Models.DTO;

namespace api_amanda.Controllers
{
    [RoutePrefix("stock")]
    public class EntradaProductoController : ApiController
    {
        [HttpPost]
        [Route("entrada")]
        public IHttpActionResult RegistrarEntrada([FromBody] EntradaStockDto dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos");

            using (var db = new AMANDAEntities())
            {
                var producto = db.PRODUCTO.Find(dto.ID_PRODUCTO);
                if (producto == null)
                    return BadRequest("Producto no encontrado");

                var proveedor = db.PROVEEDOR.Find(dto.ID_PROVEEDOR);
                if (proveedor == null)
                    return BadRequest("Proveedor no encontrado");

                var usuario = db.USUARIO.Find(dto.ID_USUARIO);
                if (usuario == null)
                    return BadRequest("Usuario inválido");

                // Registrar entrada
                var entrada = new ENTRADA_PRODUCTO
                {
                    CANTIDAD = dto.CANTIDAD,
                    FECHA = DateTime.Now,
                    ID_USUARIO = dto.ID_USUARIO,
                    ID_PROVEEDOR = dto.ID_PROVEEDOR,
                    ID_PRODUCTO = dto.ID_PRODUCTO,
                    SKU = producto.SKU
                };

                db.ENTRADA_PRODUCTO.Add(entrada);

                // Actualizar stock del producto
                producto.STOCK += dto.CANTIDAD;

                db.SaveChanges();

                return Ok("Entrada registrada y stock actualizado");
            }
        }
        // GET /stock/entrada/list
        // filtros opcionales: ?desde=2025-01-01&hasta=2025-01-31&idProducto=1&idProveedor=2
        [HttpGet]
        [Route("entrada/list")]
        public IHttpActionResult ListarEntradas(
            DateTime? desde = null,
            DateTime? hasta = null,
            int? idProducto = null,
            int? idProveedor = null)
        {
            using (var db = new AMANDAEntities())
            {
                var query = db.ENTRADA_PRODUCTO.AsQueryable();

                if (desde.HasValue)
                    query = query.Where(e => e.FECHA >= desde.Value);

                if (hasta.HasValue)
                    query = query.Where(e => e.FECHA <= hasta.Value);

                if (idProducto.HasValue)
                    query = query.Where(e => e.ID_PRODUCTO == idProducto.Value);

                if (idProveedor.HasValue)
                    query = query.Where(e => e.ID_PROVEEDOR == idProveedor.Value);

                var list = query
                    .OrderByDescending(e => e.FECHA)
                    .Select(e => new
                     {
                         id = e.ID_ENTRADA_PRODUCTO,
                        producto = e.PRODUCTO.NOMBRE,
                        proveedor = e.PROVEEDOR.NOMBRE,
                        cantidad = e.CANTIDAD,
                        fecha = e.FECHA,
                        usuario = e.USUARIO.NOMBRE,
                        anulada = e.ANULADA
                    })
                    .ToList();

                return Ok(list);
            }
        }
        [HttpPut]
        [Route("entrada/anular/{id}")]
        public IHttpActionResult AnularEntrada(int id, int idUsuario)
        {
            using (var db = new AMANDAEntities())
            {
                // Validar rol del usuario
                var esAdmin = db.ROL_USUARIO.Any(r => r.ID_USUARIO == idUsuario && r.SLUG == "admin");

                if (!esAdmin)
                    return Unauthorized(); // 401

                var entrada = db.ENTRADA_PRODUCTO.Find(id);
                if (entrada == null)
                    return NotFound();

                if (entrada.ANULADA == true)
                    return BadRequest("La entrada ya está anulada.");

                var producto = db.PRODUCTO.Find(entrada.ID_PRODUCTO);
                if (producto == null)
                    return BadRequest("Producto asociado no encontrado.");

                // Restar stock
                producto.STOCK -= entrada.CANTIDAD;
                entrada.ANULADA = true;

                db.SaveChanges();

                return Ok("Entrada anulada y stock actualizado");
            }
        }
    }

}
