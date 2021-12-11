using eCommerceAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace eCommerceAPI.Repositories
{
    public class UsuarioProcedureRepository : IUsuarioRepository
    {
        private readonly IDbConnection _connection;
        public UsuarioProcedureRepository()
        {
            _connection =
                new SqlConnection(@"Server=NTBK-BH2749\SQLEXPRESS;Database=eCommerce;Trusted_Connection=True;");
        }

        public List<Usuario> Get()
        {
            List<Usuario> usuarios = new List<Usuario>();

            try
            {
                SqlCommand command = new SqlCommand();
                command.Connection = (SqlConnection)_connection;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SelecionarUsuarios";

                _connection.Open();

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    Usuario usuario = new Usuario();

                    usuario.Id = dataReader.GetInt32("Id");
                    usuario.Nome = dataReader.GetString("Nome");
                    usuario.Email = dataReader.GetString("Email");
                    usuario.Sexo = dataReader.GetString("Sexo");
                    usuario.RG = dataReader.GetString("RG");
                    usuario.CPF = dataReader.GetString("CPF");
                    usuario.NomeMae = dataReader.GetString("NomeMae");
                    usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");
                    usuario.DataCadastro = dataReader.GetDateTimeOffset(8);

                    usuarios.Add(usuario);
                }
            }
            finally
            {
                _connection.Close();
            }

            return usuarios;
        }

        public Usuario Get(int id)
        {
            try
            {
                SqlCommand command = new SqlCommand();

                command.CommandText = "SelecionarUsuario";
                command.Parameters.AddWithValue("@Id", id);
                command.CommandType = CommandType.StoredProcedure;
                command.Connection = (SqlConnection)_connection;

                _connection.Open();
                SqlDataReader dataReader = command.ExecuteReader();

                Dictionary<int, Usuario> usuarioDictionary = new Dictionary<int, Usuario>();

                while (dataReader.Read())
                {
                    Usuario usuario = new Usuario();
                    usuario.Id = dataReader.GetInt32(0);
                    usuario.Nome = dataReader.GetString("Nome");
                    usuario.Email = dataReader.GetString("Email");
                    usuario.Sexo = dataReader.GetString("Sexo");
                    usuario.RG = dataReader.GetString("RG");
                    usuario.CPF = dataReader.GetString("CPF");
                    usuario.NomeMae = dataReader.GetString("NomeMae");
                    usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");
                    usuario.DataCadastro = dataReader.GetDateTimeOffset(8);
                }

                try
                {
                    return usuarioDictionary[usuarioDictionary.Keys.FirstOrDefault()];
                }
                catch (Exception)
                {
                    return null;
                }
            }
            finally
            {
                _connection.Close();
            }

        }

        public Usuario Insert(Usuario usuario)
        {
            _connection.Open();
            SqlTransaction transaction = (SqlTransaction)_connection.BeginTransaction();

            try
            {
                #region regionconfig
                SqlCommand command = new SqlCommand();
                command.Connection = (SqlConnection)_connection;
                #endregion

                command.CommandText = "CadastrarUsuario";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@nome", usuario.Nome);
                command.Parameters.AddWithValue("@email", usuario.Email);
                command.Parameters.AddWithValue("@sexo", usuario.Sexo);
                command.Parameters.AddWithValue("@rg", usuario.RG);
                command.Parameters.AddWithValue("@cpf", usuario.CPF);
                command.Parameters.AddWithValue("@nomeMae", usuario.NomeMae);
                command.Parameters.AddWithValue("@situacaoCadastro", usuario.SituacaoCadastro);
                command.Parameters.AddWithValue("@dataCadastro", usuario.DataCadastro);

                usuario.Id = (int)command.ExecuteScalar();
            }
            finally
            {
                _connection.Close();
            }


            return usuario;
        }

        public void Update(Usuario usuario)
        {
            _connection.Open();

            try
            {

                SqlCommand command = new SqlCommand();
                command.CommandText = "AtualizarUsuario";
                command.CommandType = CommandType.StoredProcedure;
                command.Connection = (SqlConnection)_connection;

                command.Parameters.AddWithValue("@nome", usuario.Nome);
                command.Parameters.AddWithValue("@email", usuario.Email);
                command.Parameters.AddWithValue("@sexo", usuario.Sexo);
                command.Parameters.AddWithValue("@rg", usuario.RG);
                command.Parameters.AddWithValue("@cpf", usuario.CPF);
                command.Parameters.AddWithValue("@nomeMae", usuario.NomeMae);
                command.Parameters.AddWithValue("@situacaoCadastro", usuario.SituacaoCadastro);
                command.Parameters.AddWithValue("@dataCadastro", usuario.DataCadastro);

                command.Parameters.AddWithValue("@id", usuario.Id);

                command.ExecuteNonQuery();
            }
            finally
            {
                _connection.Close();
            }
        }
        public void Deletar(int id)
        {
            try
            {
                SqlCommand command = new SqlCommand();
                command.CommandText = "DeletarUsuario";
                command.CommandType= CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@id", id);

                command.Connection = (SqlConnection)_connection;

                _connection.Open();

                command.ExecuteNonQuery();
            }
            finally
            {
                _connection.Close();
            }
        }

    }
}
