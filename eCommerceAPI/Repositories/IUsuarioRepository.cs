using eCommerceAPI.Models;
using System.Collections.Generic;

namespace eCommerceAPI.Repositories
{
    public interface IUsuarioRepository
    {
        public List<Usuario> Get();
        public Usuario Get(int id);
        public Usuario Insert(Usuario usuario);
        public void Update(Usuario usuario);
        public void Deletar(int id);
    }
}
