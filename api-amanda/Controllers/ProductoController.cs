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
    //[EnableCors(origins: "http://localhost:5173", headers: "*", methods: "*")]
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
        //[HttpGet]
        //[Route("precio/{sku}")]
        //public IHttpActionResult GetPrecio(string sku)
        //{
        //    using (var db = new AMANDAEntities())
        //    {
        //        var prod = db.PRODUCTO
        //                     .Where(p => p.SKU == sku)
        //                     .Select(p => new { p.NOMBRE, p.PRECIO })
        //                     .FirstOrDefault();

        //        if (prod == null)
        //            return NotFound();

        //        return Ok(prod);
        //    }
        //}
        [HttpGet]
        [Route("precio")]
        public IHttpActionResult GetPrecio(string sku)
        {
            using (var db = new AMANDAEntities())
            {
                RESPUESTA resp = new RESPUESTA();
                var p = db.PRODUCTO.FirstOrDefault(x => x.SKU == sku);

                if (p != null)
                {
                    resp.sku = p.SKU;
                    resp.nombre = p.NOMBRE;
                    resp.precio = p.PRECIO;
                    resp.tipo = "Otro";
                    resp.stock = p.STOCK;
                    return Ok(resp);
                }

                // Intentar PACK
                var pk = db.PACK.FirstOrDefault(x => x.SKU_PACK == sku);
                if (pk != null)
                {
                    resp.sku = pk.SKU_PACK;
                    resp.nombre = pk.NOMBRE_PACK;
                    resp.precio = pk.PRECIO_PACK;
                    resp.tipo = "PACK";
                    return Ok(resp);
                }

                return NotFound();
            }
        }
        [HttpGet]
        [Route("search")]
        public IHttpActionResult GetProductosFiltrados(string search = null, int? tipoId = null, int? proveedorId = null, int page = 1, int pageSize = 10, string sortField = "NOMBRE", string sortOrder = "asc")
        {
            using (var db = new AMANDAEntities())
            {

                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 100) pageSize = 10;

                var query = db.PRODUCTO.AsQueryable();

                // 🔍 BÚSQUEDA GLOBAL
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p =>
                        p.SKU.Contains(search) ||
                        p.NOMBRE.Contains(search) ||
                        p.DESCRIPCION.Contains(search) ||
                        p.PROVEEDOR.NOMBRE.Contains(search) ||
                        p.TIPO_PRODUCTO.NOMBRE.Contains(search)
                    );
                }

                // 🏷️ FILTRO TIPO PRODUCTO
                if (tipoId.HasValue)
                    query = query.Where(p => p.ID_TIPO_PRODUCTO == tipoId.Value);

                // 🚚 FILTRO PROVEEDOR
                if (proveedorId.HasValue)
                    query = query.Where(p => p.ID_PROVEEDOR == proveedorId.Value);

                // 📊 TOTAL (para paginación)
                var total = query.Count();

                switch (sortField)
                {
                    case "PRECIO":
                        query = sortOrder == "desc"
                            ? query.OrderByDescending(p => p.PRECIO)
                            : query.OrderBy(p => p.PRECIO);
                        break;

                    case "STOCK":
                        query = sortOrder == "desc"
                            ? query.OrderByDescending(p => p.STOCK)
                            : query.OrderBy(p => p.STOCK);
                        break;

                    case "SKU":
                        query = sortOrder == "desc"
                            ? query.OrderByDescending(p => p.SKU)
                            : query.OrderBy(p => p.SKU);
                        break;

                    default: // NOMBRE
                        query = sortOrder == "desc"
                            ? query.OrderByDescending(p => p.NOMBRE)
                            : query.OrderBy(p => p.NOMBRE);
                        break;
                }
                // 📄 PAGINACIÓN REAL
                var data = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
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
                    })
                    .ToList();

                return Ok(new
                {
                    page,
                    pageSize,
                    total,
                    data
                });
            }
        }
        [HttpGet]
        [Route("export")]
        public HttpResponseMessage ExportarProductosExcel(
    string search = null,
    int? tipoId = null,
    int? proveedorId = null,
    string sortField = "NOMBRE",
    string sortOrder = "asc"
)
        {
            using (var db = new AMANDAEntities())
            {
                var query = db.PRODUCTO.AsQueryable();

                // 🔍 filtros
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p =>
                        p.SKU.Contains(search) ||
                        p.NOMBRE.Contains(search) ||
                        p.DESCRIPCION.Contains(search) ||
                        p.PROVEEDOR.NOMBRE.Contains(search) ||
                        p.TIPO_PRODUCTO.NOMBRE.Contains(search)
                    );
                }

                if (tipoId.HasValue)
                    query = query.Where(p => p.ID_TIPO_PRODUCTO == tipoId);

                if (proveedorId.HasValue)
                    query = query.Where(p => p.ID_PROVEEDOR == proveedorId);

                // 🔽 ordenamiento
                switch (sortField)
                {
                    case "PRECIO":
                        query = sortOrder == "desc"
                            ? query.OrderByDescending(p => p.PRECIO)
                            : query.OrderBy(p => p.PRECIO);
                        break;

                    case "STOCK":
                        query = sortOrder == "desc"
                            ? query.OrderByDescending(p => p.STOCK)
                            : query.OrderBy(p => p.STOCK);
                        break;

                    case "SKU":
                        query = sortOrder == "desc"
                            ? query.OrderByDescending(p => p.SKU)
                            : query.OrderBy(p => p.SKU);
                        break;

                    default:
                        query = sortOrder == "desc"
                            ? query.OrderByDescending(p => p.NOMBRE)
                            : query.OrderBy(p => p.NOMBRE);
                        break;
                }

                var data = query.Select(p => new
                {
                    p.SKU,
                    p.NOMBRE,
                    p.PRECIO,
                    p.STOCK,
                    Tipo = p.TIPO_PRODUCTO.NOMBRE,
                    Proveedor = p.PROVEEDOR.NOMBRE,
                    ExentoIVA = (bool)p.EXCENTO_IVA ? "Sí" : "No",
                    Alerta = p.ALERTA_STOCK.NOMBRE
                }).ToList();

                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Productos");

                    // Cabeceras
                    ws.Cell(1, 1).Value = "SKU";
                    ws.Cell(1, 2).Value = "Nombre";
                    ws.Cell(1, 3).Value = "Precio";
                    ws.Cell(1, 4).Value = "Stock";
                    ws.Cell(1, 5).Value = "Tipo";
                    ws.Cell(1, 6).Value = "Proveedor";
                    ws.Cell(1, 7).Value = "Exento IVA";
                    ws.Cell(1, 8).Value = "Alerta";

                    ws.Row(1).Style.Font.Bold = true;

                    int row = 2;
                    foreach (var p in data)
                    {
                        ws.Cell(row, 1).Value = p.SKU;
                        ws.Cell(row, 2).Value = p.NOMBRE;
                        ws.Cell(row, 3).Value = p.PRECIO;
                        ws.Cell(row, 4).Value = p.STOCK;
                        ws.Cell(row, 5).Value = p.Tipo;
                        ws.Cell(row, 6).Value = p.Proveedor;
                        ws.Cell(row, 7).Value = p.ExentoIVA;
                        ws.Cell(row, 8).Value = p.Alerta;
                        row++;
                    }

                    ws.Columns().AdjustToContents();

                    var stream = new System.IO.MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    var result = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(stream.ToArray())
                    };

                    result.Content.Headers.ContentDisposition =
                        new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                        {
                            FileName = "Productos.xlsx"
                        };

                    result.Content.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        );

                    return result;
                }
            }
        }
        [HttpGet]
        [Route("suggest")]
        public IHttpActionResult SugerirProductos(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
                return Ok(new List<object>());

            using (var db = new AMANDAEntities())
            {
                var productos = db.PRODUCTO
                    .Where(p =>
                        p.NOMBRE.Contains(term) ||
                        p.SKU.Contains(term)
                    )
                    .OrderBy(p => p.NOMBRE)
                    .Take(15)
                    .Select(p => new
                    {
                        tipo = "producto",
                        id = p.ID_PRODUCTO,
                        sku = p.SKU,
                        nombre = p.NOMBRE,
                        precio = p.PRECIO,
                        exento = p.EXCENTO_IVA,
                        typeName = p.TIPO_PRODUCTO.NOMBRE
                    });

                var packs = db.PACK
                    .Where(p =>
                        p.NOMBRE_PACK.Contains(term) ||
                        p.SKU_PACK.Contains(term)
                    )
                    .OrderBy(p => p.NOMBRE_PACK)
                    .Take(10)
                    .Select(p => new
                    {
                        tipo = "pack",
                        id = p.ID_PACK,
                        sku = p.SKU_PACK,
                        nombre = p.NOMBRE_PACK,
                        precio = p.PRECIO_PACK,
                        exento = p.EXCENTO_IVA,
                        typeName = "pack"
                    });

                return Ok(productos.Concat(packs).Take(20).ToList());
            }
        }

        public class RESPUESTA
        {
            public string sku { get; set; }
            public string nombre { get; set; }
            public int? precio { get; set; }
            public string tipo { get; set; }
            public decimal? stock { get; set; }
        }
    }
}
