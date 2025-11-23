using System;
using System.Linq;
using System.Web.Http;
using api_amanda.Models;
using api_amanda.Models.DTO;

namespace api_amanda.Controllers
{
    [RoutePrefix("ventas")]
    public class VentaController : ApiController
    {
        // POST /ventas
        [HttpPost]
        [Route("")]
        public IHttpActionResult CrearVenta([FromBody] VentaDto dto)
        {

            if (dto == null || dto.Items == null || !dto.Items.Any())
                return BadRequest("No hay ítems en la venta.");

            using (var db = new AMANDAEntities())
            {

                // Validar usuario
                var usuario = db.USUARIO.Find(dto.IdUsuario);
                if (usuario == null)
                    return BadRequest("Usuario no válido.");

                var cajaActiva = db.APERTURA_CIERRE.FirstOrDefault(a => a.ID_USUARIO == dto.IdUsuario && a.ACTIVA == true);

                if (cajaActiva == null)
                    return BadRequest("No puede vender sin tener una caja abierta.");


                dto.IdCaja = cajaActiva.ID_CAJA;
                // Calcular total y validar stock
                decimal total = 0;
                bool contieneCigarros = false;

                foreach (var item in dto.Items)
                {
                    if (item.Cantidad <= 0)
                        return BadRequest("Cantidad inválida en un ítem.");

                    decimal subtotal = item.Cantidad * item.PrecioUnitario;
                    total += subtotal;

                    if (item.Tipo == "producto")
                    {
                        var prod = db.PRODUCTO.Find(item.Id);
                        if (prod == null)
                            return BadRequest("Producto de la venta no existe.");

                        // Verificar stock suficiente
                        if (prod.STOCK < item.Cantidad)
                            return BadRequest($"Stock insuficiente para producto {prod.NOMBRE}.");

                        // Marcar si es cigarrillo
                        if (prod.TIPO_PRODUCTO != null && prod.TIPO_PRODUCTO.CIGARRILLO == true)
                            contieneCigarros = true;
                    }
                    else if (item.Tipo == "pack")
                    {
                        var pack = db.PACK.Find(item.Id);
                        if (pack == null)
                            return BadRequest("Pack de la venta no existe.");

                        // Verificar stock suficiente por cada producto del pack
                        var detallesPack = db.PACK_DETALLE.Where(d => d.ID_PACK == pack.ID_PACK).ToList();
                        foreach (var det in detallesPack)
                        {
                            var prod = det.PRODUCTO;
                            if (prod == null)
                                return BadRequest("Producto de pack no encontrado.");

                            var totalUnidades = det.CANTIDAD_PRODUCTO * item.Cantidad;
                            if (prod.STOCK < totalUnidades)
                                return BadRequest($"Stock insuficiente para producto {prod.NOMBRE} del pack {pack.NOMBRE_PACK}.");

                            if (prod.TIPO_PRODUCTO != null && prod.TIPO_PRODUCTO.CIGARRILLO == true)
                                contieneCigarros = true;
                        }
                    }
                    else
                    {
                        return BadRequest("Tipo de ítem inválido (use 'producto' o 'pack').");
                    }
                }

                // Aplicar descuento
                total -= dto.DescuentoAplicado;
                if (total < 0) total = 0;

                // Crear cabecera VENTA
                var venta = new VENTA
                {
                    FECHA = DateTime.Now,
                    TOTAL = total,
                    DESCUENTO_APLICADO = dto.DescuentoAplicado,
                    CONTIENE_CIGARROS = contieneCigarros,
                    ID_USUARIO = dto.IdUsuario,
                    ID_CAJA = cajaActiva.ID_CAJA,
                    ID_APERTURA_CIERRE = cajaActiva.ID_APERTURA_CIERRE
                };

                db.VENTA.Add(venta);
                db.SaveChanges(); // para obtener ID_VENTA

                // Crear detalles y actualizar stock
                foreach (var item in dto.Items)
                {
                    if (item.Tipo == "producto")
                    {
                        var prod = db.PRODUCTO.Find(item.Id);

                        // Guardar stock antes
                        var stockAntes = prod.STOCK;

                        // Descontar stock
                        prod.STOCK -= item.Cantidad;

                        // Registrar historial
                        db.HISTORIAL_STOCK.Add(new HISTORIAL_STOCK
                        {
                            ID_PRODUCTO = prod.ID_PRODUCTO,
                            TIPO_MOVIMIENTO = "Venta",
                            CANTIDAD = -item.Cantidad,
                            STOCK_ANTERIOR = (decimal)stockAntes,
                            STOCK_NUEVO = (decimal)prod.STOCK,
                            FECHA = DateTime.Now,
                            USUARIO = usuario.NOMBRE,
                            OBSERVACION = "Venta POS"
                        });

                        // Registrar detalle de venta
                        var det = new DETALLE_VENTA
                        {
                            SKU = prod.SKU,
                            NOMBRE = prod.NOMBRE,
                            CANTIDAD = item.Cantidad,
                            PRECIO_UNITARIO = (int)item.PrecioUnitario,
                            SUBTOTAL = item.Cantidad * item.PrecioUnitario,
                            TIENE_PROMOCION = false,
                            FECHA_CREACION = DateTime.Now,
                            USUARIO_VENTA = dto.IdUsuario,
                            EXCENTO_IVA = item.ExcentoIva,
                            ID_VENTA = venta.ID_VENTA,
                            ID_PRODUCTO = prod.ID_PRODUCTO
                        };

                        db.DETALLE_VENTA.Add(det);
                    }
                    else if (item.Tipo == "pack")
                    {
                        var pack = db.PACK.Find(item.Id);

                        // Descontar stock de productos internos
                        var detallesPack = db.PACK_DETALLE.Where(d => d.ID_PACK == pack.ID_PACK).ToList();
                        foreach (var detPack in detallesPack)
                        {
                            var prod = detPack.PRODUCTO;
                            var totalUnidades = detPack.CANTIDAD_PRODUCTO * item.Cantidad;

                            // Guardar stock antes
                            var stockAntes = prod.STOCK;

                            // Descontar unidades del producto
                            prod.STOCK -= totalUnidades;

                            // Registrar historial
                            db.HISTORIAL_STOCK.Add(new HISTORIAL_STOCK
                            {
                                ID_PRODUCTO = prod.ID_PRODUCTO,
                                TIPO_MOVIMIENTO = "Venta Pack",
                                CANTIDAD = -(decimal)totalUnidades,
                                STOCK_ANTERIOR = (decimal)stockAntes,
                                STOCK_NUEVO = (decimal)prod.STOCK,
                                FECHA = DateTime.Now,
                                USUARIO = usuario.NOMBRE,
                                OBSERVACION = "Pack: " + pack.NOMBRE_PACK
                            });
                        }

                        // Registrar detalle de venta como PACK solamente
                        var det = new DETALLE_VENTA
                        {
                            SKU = pack.SKU_PACK,
                            NOMBRE = pack.NOMBRE_PACK,
                            CANTIDAD = item.Cantidad,
                            PRECIO_UNITARIO = (int)item.PrecioUnitario,
                            SUBTOTAL = item.Cantidad * item.PrecioUnitario,
                            TIENE_PROMOCION = false,
                            FECHA_CREACION = DateTime.Now,
                            USUARIO_VENTA = dto.IdUsuario,
                            EXCENTO_IVA = item.ExcentoIva,
                            ID_VENTA = venta.ID_VENTA,
                            ID_PRODUCTO = null,
                            ID_PROMOCION = null
                        };

                        db.DETALLE_VENTA.Add(det);
                    }
                }

                db.SaveChanges();

                return Ok(new
                {
                    mensaje = "Venta registrada correctamente",
                    idVenta = venta.ID_VENTA,
                    total = venta.TOTAL
                });
            }
        }
    }
}
