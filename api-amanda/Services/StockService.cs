using System;
using System.Linq;
using api_amanda.Models;
using System.Data;

namespace api_amanda.Services
{
    public class StockService
    {
        private readonly AMANDAEntities _db;

        public StockService()
        {
            _db = new AMANDAEntities();
        }

        // ============================================================
        //              ENTRADA DE PRODUCTO (COMPRAS)
        // ============================================================
        public void RegistrarEntrada(int idProducto, decimal cantidad, decimal costoUnitario, int idUsuario, int idProveedor, string observacion = "Entrada de producto")
        {
            _db.Database.Connection.Open();
            using (var tx = _db.Database.Connection.BeginTransaction())
            {
                try
                {
                    var prod = _db.PRODUCTO.Find(idProducto);
                    if (prod == null)
                        throw new Exception("Producto no encontrado.");

                    var producto = _db.PRODUCTO.Find(idProducto);
                    if (producto == null)
                        throw new Exception("Producto no encontrado");

                    var proveedor = _db.PROVEEDOR.Find(idProveedor);
                    if (proveedor == null)
                        throw new Exception("Proveedor no encontrado");

                    var usuario = _db.USUARIO.Find(idUsuario);
                    if (usuario == null)
                        throw new Exception("Usuario inválido");

                    var stockAntes = prod.STOCK;
                    prod.STOCK += cantidad;

                    _db.ENTRADA_PRODUCTO.Add(new ENTRADA_PRODUCTO
                    {
                        ID_PRODUCTO = idProducto,
                        CANTIDAD = cantidad,
                        COSTO_UNITARIO = costoUnitario,
                        FECHA = DateTime.Now,
                        ID_PROVEEDOR = idProveedor,
                        ID_USUARIO = idUsuario,
                        SKU = prod.SKU,
                        ANULADA = false
                    });

                    RegistrarHistorial(prod.ID_PRODUCTO, "ENTRADA PRODUCTO", cantidad, (decimal)stockAntes, (decimal)prod.STOCK, idUsuario.ToString(), observacion);

                    _db.SaveChanges();
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw new Exception("Error registrando entrada: " + ex.Message);
                }
            }
        }

        // ============================================================
        //                    AJUSTE MANUAL DE STOCK
        // ============================================================
        public void AjustarStock(int idProducto, decimal nuevoStock, int idUsuario, string observacion = "Ajuste manual de stock")
        {
            _db.Database.Connection.Open();
            using (var tx = _db.Database.Connection.BeginTransaction())
            {
                try
                {
                    var prod = _db.PRODUCTO.Find(idProducto);
                    if (prod == null)
                        throw new Exception("Producto no encontrado.");

                    var stockAnterior = prod.STOCK;
                    var diferencia = nuevoStock - stockAnterior;

                    prod.STOCK = nuevoStock;

                    RegistrarHistorial(prod.ID_PRODUCTO,
                        diferencia >= 0 ? "AJUSTE (+)" : "AJUSTE (-)",
                        (decimal)diferencia,
                        (decimal)stockAnterior,
                        nuevoStock,
                        idUsuario.ToString(),
                        observacion
                    );

                    _db.SaveChanges();
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw new Exception("Error ajustando stock: " + ex.Message);
                }
            }
        }

        // ============================================================
        //                DEVOLUCIÓN / ANULACIÓN DE VENTA
        // ============================================================
        public void RevertirVenta(int idVenta, int idUsuario)
        {
            _db.Database.Connection.Open();
            using (var tx = _db.Database.Connection.BeginTransaction())
            {
                try
                {
                    var venta = _db.VENTA.Find(idVenta);
                    if (venta == null)
                        throw new Exception("No existe la venta para revertir.");

                    var detalles = _db.DETALLE_VENTA.Where(d => d.ID_VENTA == idVenta).ToList();

                    foreach (var det in detalles)
                    {
                        // Si es producto directo
                        if (det.ID_PRODUCTO != null)
                        {
                            var prod = _db.PRODUCTO.Find(det.ID_PRODUCTO);
                            var stockAntes = prod.STOCK;
                            prod.STOCK += det.CANTIDAD;

                            RegistrarHistorial(prod.ID_PRODUCTO, "ANULACIÓN VENTA", (decimal)det.CANTIDAD, (decimal)stockAntes, (decimal)prod.STOCK, idUsuario.ToString(), $"Anulación venta ID {idVenta}");
                        }
                        else
                        {
                            // Si es pack, devolver productos internos
                            var pack = _db.PACK.FirstOrDefault(p => p.SKU_PACK == det.SKU);
                            if (pack != null)
                            {
                                var detallesPack = _db.PACK_DETALLE.Where(p => p.ID_PACK == pack.ID_PACK).ToList();
                                foreach (var d in detallesPack)
                                {
                                    var prod = _db.PRODUCTO.Find(d.ID_PRODUCTO);
                                    var unidadesADevolver = det.CANTIDAD * d.CANTIDAD_PRODUCTO;

                                    var stockAntes = prod.STOCK;
                                    prod.STOCK += unidadesADevolver;

                                    RegistrarHistorial(prod.ID_PRODUCTO, "ANULACIÓN PACK", (decimal)unidadesADevolver, (decimal)stockAntes, (decimal)prod.STOCK, idUsuario.ToString(), $"Anulación pack de venta ID {idVenta}");
                                }
                            }
                        }
                    }

                    _db.SaveChanges();
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw new Exception("Error revertiendo venta: " + ex.Message);
                }
            }
        }

        // ============================================================
        //             MÉTODO CENTRAL DE HISTORIAL
        // ============================================================
        private void RegistrarHistorial(int idProducto, string tipo, decimal cantidad, decimal stockAntes, decimal stockNuevo, string usuario, string observacion)
        {
            _db.HISTORIAL_STOCK.Add(new HISTORIAL_STOCK
            {
                ID_PRODUCTO = idProducto,
                TIPO_MOVIMIENTO = tipo,
                CANTIDAD = cantidad,
                STOCK_ANTERIOR = stockAntes,
                STOCK_NUEVO = stockNuevo,
                FECHA = DateTime.Now,
                USUARIO = usuario,
                OBSERVACION = observacion
            });
        }
    }
}