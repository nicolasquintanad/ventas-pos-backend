using api_amanda.Models;
using api_amanda.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace api_amanda.Controllers
{
    [RoutePrefix("products")]
    [EnableCors(origins: "http://localhost:5173", headers: "*", methods: "*")]
    public class ProductoController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetProductos()
        {
            using (var db = new AMANDAEntities())
            {
                var productos = db.PRODUCTO.Select(p => new
                {
                    id = p.ID_PRODUCTO,
                    sku = p.SKU,
                    name = p.NOMBRE,
                    description = p.DESCRIPCION,
                    priceUnit = p.PRECIO,
                    stockUnits = p.STOCK,
                    exempt = p.EXCENTO_IVA,
                    typeId = p.ID_TIPO_PRODUCTO,
                    typeName = p.TIPO_PRODUCTO.NOMBRE,
                    alertaNombre = p.ALERTA_STOCK.NOMBRE,
                    ID_ALERTA = p.ID_ALERTA,
                    proveedorNombre = p.PROVEEDOR.NOMBRE,
                    ID_PROVEEDOR = p.ID_PROVEEDOR
                }).ToList();

                return Ok(productos);
            }
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateProducto(ProductoDto dto)
        {
            if (dto == null) return Content(HttpStatusCode.BadRequest, new Error("Datos inválidos"));

            using (var db = new AMANDAEntities())
            {
                var producto = db.PRODUCTO.Where(x => x.SKU == dto.SKU).FirstOrDefault();
                if (producto!=null)
                {
                    return Content(HttpStatusCode.BadRequest, new Error("SKU ya está asociado a un producto anterior"));
                }
                var p = new PRODUCTO
                {
                    SKU = dto.SKU,
                    NOMBRE = dto.NOMBRE,
                    DESCRIPCION = dto.DESCRIPCION,
                    PRECIO = dto.PRECIO,
                    STOCK = dto.STOCK,
                    EXCENTO_IVA = dto.EXCENTO_IVA,
                    ID_TIPO_PRODUCTO = dto.ID_TIPO_PRODUCTO,
                    ID_ALERTA = dto.ID_ALERTA,
                    ID_PROVEEDOR = dto.ID_PROVEEDOR
            };

                db.PRODUCTO.Add(p);
                db.SaveChanges();

                return Ok("Producto creado");
            }
        }

        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult UpdateProducto(int id, ProductoDto dto)
        {
            using (var db = new AMANDAEntities())
            {
                var product = db.PRODUCTO.Find(id);
                if (product == null) return NotFound();

                product.SKU = dto.SKU;
                product.NOMBRE = dto.NOMBRE;
                product.DESCRIPCION = dto.DESCRIPCION;
                product.PRECIO = dto.PRECIO;
                product.STOCK = dto.STOCK;
                product.EXCENTO_IVA = dto.EXCENTO_IVA;
                product.ID_TIPO_PRODUCTO = dto.ID_TIPO_PRODUCTO;
                product.ID_ALERTA = dto.ID_ALERTA;
                product.ID_PROVEEDOR = dto.ID_PROVEEDOR;

                db.SaveChanges();

                return Ok("Producto actualizado");
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteProducto(int id)
        {
            using (var db = new AMANDAEntities())
            {
                var product = db.PRODUCTO.Find(id);
                if (product == null) return NotFound();

                db.PRODUCTO.Remove(product);
                db.SaveChanges();

                return Ok("Producto eliminado");
            }
        }
        [HttpGet]
        [Route("stock/{idProducto}")]
        public IHttpActionResult GetStockProducto(int idProducto)
        {
            using (var db = new AMANDAEntities())
            {
                var producto = db.PRODUCTO.Find(idProducto);

                if (producto == null)
                    return NotFound();

                return Ok(new
                {
                    id = producto.ID_PRODUCTO,
                    nombre = producto.NOMBRE,
                    stock = producto.STOCK
                });
            }
        }
        [HttpGet]
        [Route("historial/{idProducto}")]
        public IHttpActionResult GetHistorialProducto(int idProducto)
        {
            using (var db = new AMANDAEntities())
            {
                var historial = db.HISTORIAL_STOCK
                    .Where(h => h.ID_PRODUCTO == idProducto)
                    .OrderByDescending(h => h.FECHA)
                    .Select(h => new
                    {
                        h.FECHA,
                        h.TIPO_MOVIMIENTO,
                        h.CANTIDAD,
                        h.STOCK_ANTERIOR,
                        h.STOCK_NUEVO,
                        h.USUARIO,
                        h.OBSERVACION
                    }).ToList();

                return Ok(historial);
            }
        }
        [HttpGet]
        [Route("product-types")]
        public IHttpActionResult ObtenerTiposProducto()
        {
            try { 
            using (var db = new AMANDAEntities())
            {
                var tipos = db.TIPO_PRODUCTO
                    .Select(t => new
                    {
                        id = t.ID_TIPO_PRODUCTO,
                        name = t.NOMBRE,
                        cigarrillo = (bool?)t.CIGARRILLO ?? false
                    })
                    .ToList();

                return Ok(tipos);
            }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("alertas-stock")]
        public IHttpActionResult GetAlertasStock()
        {
            using (var db = new AMANDAEntities())
            {
                var alertas = db.ALERTA_STOCK
                    .Select(a => new {
                        id = a.ID_ALERTA,
                        nombre = a.NOMBRE,
                        unidades = a.UNIDADES
                    }).ToList();

                return Ok(alertas);
            }
        }
        [HttpGet]
        [Route("alertas-resumen")]
        public IHttpActionResult GetResumenAlertas()
        {
            using (var db = new AMANDAEntities())
            {
                var resumen = db.PRODUCTO
                    .GroupBy(p => p.ALERTA_STOCK.NOMBRE)
                    .Select(g => new
                    {
                        nivel = g.Key,
                        cantidad = g.Count()
                    })
                    .ToList();

                return Ok(resumen);
            }
        }
        [HttpGet]
        [Route("productos-criticos")]
        public IHttpActionResult GetProductosCriticos()
        {
            using (var db = new AMANDAEntities())
            {
                var criticos = db.PRODUCTO
                    .Where(p => p.STOCK <= p.ALERTA_STOCK.UNIDADES)
                    .Select(p => new {
                        id = p.ID_PRODUCTO,
                        sku = p.SKU,
                        nombre = p.NOMBRE,
                        stock = p.STOCK,
                        nivel = p.ALERTA_STOCK.NOMBRE,
                        minimo = p.ALERTA_STOCK.UNIDADES
                    }).ToList();

                return Ok(criticos);
            }
        }
    }
}
