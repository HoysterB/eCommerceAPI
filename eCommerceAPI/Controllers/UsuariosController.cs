using eCommerceAPI.Models;
using eCommerceAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace eCommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepository _repository;
        public UsuariosController(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult PegarTodos()
        {
            var ListUsuario = _repository.Get();

            var list = ListUsuario.Select(x => new
            {
                mensagem = "Sucesso ao retornar todos os usuários",
                Usuario = new
                {
                    x.Id,
                    x.Nome,
                    x.RG
                }

            });

            return Ok(list);
        }

        [HttpGet("{id}")]
        public IActionResult PegarPorId(int id)
        {
            var usuario = _repository.Get(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        [HttpPost]
        public IActionResult CadastrarUsuario(Usuario usuario)
        {
            try
            {
                _repository.Insert(usuario);

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut]
        public IActionResult AlterarUsuario(Usuario usuario)
        {
            try
            {
                _repository.Update(usuario);

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public IActionResult DeletarUsuario(int id)
        {
            _repository.Deletar(id);
            return Ok();
        }
    }
}
