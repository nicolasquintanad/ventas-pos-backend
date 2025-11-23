using System.Linq;
using api_amanda.Models;

namespace api_amanda.Helpers
{
    public static class Permisos
    {
        public static bool EsAdmin(int idUsuario)
        {
            using (var db = new AMANDAEntities())
            {
                var rol = db.ROL_USUARIO.FirstOrDefault(r => r.ID_USUARIO == idUsuario);
                return rol != null && rol.SLUG == "admin";
            }
        }
    }
}