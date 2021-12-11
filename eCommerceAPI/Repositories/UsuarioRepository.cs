using eCommerceAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

/*
 * ADO.NET possui 4 principais classes
 * 
 * Connection => Estabelecer conexão com o banco
 * 
 * Command => INSERT, UPDATE, DELETE -> Serve para executar comandos no banco e esses comandos geralmente não 
 * tem um retorno ou tem poucas informações para retornar
 * 
 * DataReader => Arq. Conectada -> por exemplo, a gente faz um select nele, ele vai no banco de dados, ele se conecta,
 * e a gente percorre os elementos tudo no banco, é uma arquitetura conectada, eu sempre vou estar conectado ao banco
 * ao fazer um select
 * 
 * DataAdapter => Arquitetura Desconectada -> Significa que, se eu executar o mesmo select, esse select executa no banco de dados,
 * só que ele percorre e guarda na memória do computador e desconecta do banco de dados, ele traz os dados consultados para a memória 
 * do computador, depois disso ele desconecta do banco, não precisa ficar conectado
 */
namespace eCommerceAPI.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnection _connection;
        public UsuarioRepository()
        {
            _connection =
                new SqlConnection(@"Server=NTBK-BH2749\SQLEXPRESS;Database=eCommerce;Trusted_Connection=True;");
        }
        public void Deletar(int id)
        {
            try
            {
                SqlCommand command = new SqlCommand();
                command.CommandText = "DELETE FROM Usuarios WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                command.Connection = (SqlConnection)_connection;

                _connection.Open();

                command.ExecuteNonQuery();
            }
            finally
            {
                _connection.Close();
            }
        }

        public List<Usuario> Get()
        {
            List<Usuario> usuarios = new List<Usuario>();

            try
            {
                SqlCommand command = new SqlCommand();

                command.CommandText = "SELECT * FROM Usuarios";
                command.Connection = (SqlConnection)_connection;

                _connection.Open();

                SqlDataReader dataReader = command.ExecuteReader();

                //dataReader.Read();// é um ponteiro, que lê linhas da tabela, verifica se há uma linha e faz o ponteiro andar
                //Obs: quando eu testei usar o dataReader.Read() fora do while, como mostra
                //no código, ele pula o 1° registro!!

                while (dataReader.Read()) //enquanto for true, vai iterar, enquanto houverem linhas na tabela ele irá retornar true, quando retornar false não haverá mais linha na tabela 
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

                command.CommandText = $"SELECT * FROM Usuarios u LEFT JOIN Contatos c ON c.UsuarioId = u.Id LEFT JOIN EnderecosEntrega ee ON ee.UsuarioId = u.Id LEFT JOIN UsuariosDepartamentos ud ON ud.UsuarioId = u.Id LEFT JOIN Departamentos d ON d.Id = ud.DepartamentoId WHERE u.Id = @Id;";
                command.Parameters.AddWithValue("@Id", id);
                command.Connection = (SqlConnection)_connection;

                _connection.Open();
                SqlDataReader dataReader = command.ExecuteReader();

                Dictionary<int, Usuario> usuarioDictionary = new Dictionary<int, Usuario>();

                while (dataReader.Read())
                {
                    Usuario usuario = new Usuario();

                    if (!(usuarioDictionary.ContainsKey(dataReader.GetInt32(0))))
                    {
                        usuario.Id = dataReader.GetInt32(0);
                        usuario.Nome = dataReader.GetString("Nome");
                        usuario.Email = dataReader.GetString("Email");
                        usuario.Sexo = dataReader.GetString("Sexo");
                        usuario.RG = dataReader.GetString("RG");
                        usuario.CPF = dataReader.GetString("CPF");
                        usuario.NomeMae = dataReader.GetString("NomeMae");
                        usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");
                        usuario.DataCadastro = dataReader.GetDateTimeOffset(8);

                        try
                        {
                            Contato contato = new Contato();
                            contato.Id = dataReader.GetInt32(9); //dataReader me deixa pegar pelo nome ou pelo índice!
                            contato.UsuarioId = usuario.Id;
                            contato.Telefone = dataReader.GetString("Telefone");
                            contato.Celular = dataReader.GetString("Celular");

                            usuario.Contato = contato;
                        }
                        catch (Exception ex)
                        {
                            usuario.Contato = null;
                        }


                        usuarioDictionary.Add(usuario.Id, usuario);
                    }
                    else
                    {
                        usuario = usuarioDictionary[dataReader.GetInt32(0)];
                    }


                    EnderecoDeEntrega endereco = new EnderecoDeEntrega();
                    try
                    {
                        endereco.Id = dataReader.GetInt32(13);
                        endereco.UsuarioId = usuario.Id;
                        endereco.NomeEndereco = dataReader.GetString("NomeEndereco");
                        endereco.CEP = dataReader.GetString("CEP");
                        endereco.Estado = dataReader.GetString("Estado");
                        endereco.Cidade = dataReader.GetString("Cidade");
                        endereco.Bairro = dataReader.GetString("Bairro");
                        endereco.Endereco = dataReader.GetString("Endereco");
                        endereco.Numero = dataReader.GetString("Numero");
                        endereco.Complemento = dataReader.GetString("Complemento");

                        usuario.EnderecosDeEntrega = (usuario.EnderecosDeEntrega == null) ? new List<EnderecoDeEntrega>() : usuario.EnderecosDeEntrega;

                        if (usuario.EnderecosDeEntrega.FirstOrDefault(x => x.Id == endereco.Id) == null)
                        {
                            usuario.EnderecosDeEntrega.Add(endereco);
                        }
                    }
                    catch (Exception)
                    {
                        endereco = null;
                    }

                    Departamento departamento = new Departamento();

                    try
                    {
                        departamento.Id = dataReader.GetInt32(26);
                        departamento.Nome = dataReader.GetString(27);

                        usuario.Departamentos = (usuario.Departamentos == null) ? new List<Departamento>() : usuario.Departamentos;

                        if (usuario.Departamentos.FirstOrDefault(x => x.Id == departamento.Id) == null)
                        {
                            usuario.Departamentos.Add(departamento);
                        }
                    }
                    catch (Exception)
                    {

                        departamento = null;
                    }

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
                command.Transaction = transaction;
                command.Connection = (SqlConnection)_connection;
                #endregion

                command.CommandText = $"INSERT INTO Usuarios (Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) " +
                    $"VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro);" +
                    $"SELECT CAST(scope_identity() AS int)"; //seleciona o último identity baseado no escopo de execução do nosso insert! Ele também cuida no caso de concorrêcnia

                command.Parameters.AddWithValue("@Nome", usuario.Nome);
                command.Parameters.AddWithValue("@Email", usuario.Email);
                command.Parameters.AddWithValue("@Sexo", usuario.Sexo);
                command.Parameters.AddWithValue("@RG", usuario.RG);
                command.Parameters.AddWithValue("@CPF", usuario.CPF);
                command.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
                command.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
                command.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);



                //command.ExecuteNonQuery(); // não nos dá retorno, é bom para fazer insert, delete, update, etc... Onde não precisamos de um retorno!
                //temos que mudar de executenonquery para scalar para poder retornar retornar apenas a informação, a única linha que precisamos, a unica coluna

                usuario.Id = (int)command.ExecuteScalar();



                command.CommandText = "INSERT INTO Contatos (UsuarioId, Telefone, Celular) VALUES (@UsuarioId, @Telefone, @Celular);" +
                    "SELECT CAST(scope_identity() AS int)";
                command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                command.Parameters.AddWithValue("@Telefone", usuario.Contato.Telefone);
                command.Parameters.AddWithValue("@Celular", usuario.Contato.Celular);


                usuario.Contato.UsuarioId = usuario.Id;
                usuario.Contato.Id = (int)command.ExecuteScalar();


                foreach (var endereco in usuario.EnderecosDeEntrega)
                {
                    command = new SqlCommand();
                    command.Transaction = transaction;
                    command.Connection = (SqlConnection)_connection;

                    command.CommandText = "INSERT INTO EnderecosEntrega (UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES" +
                        "(@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento)" +
                        "SELECT CAST(scope_identity() AS int)";

                    command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                    command.Parameters.AddWithValue("@NomeEndereco", endereco.NomeEndereco);
                    command.Parameters.AddWithValue("@CEP", endereco.CEP);
                    command.Parameters.AddWithValue("@Estado", endereco.Estado);
                    command.Parameters.AddWithValue("@Cidade", endereco.Cidade);
                    command.Parameters.AddWithValue("@Bairro", endereco.Bairro);
                    command.Parameters.AddWithValue("@Endereco", endereco.Endereco);
                    command.Parameters.AddWithValue("@Numero", endereco.Numero);
                    command.Parameters.AddWithValue("@Complemento", endereco.Complemento);

                    endereco.Id = (int)command.ExecuteScalar();
                    endereco.UsuarioId = usuario.Id;
                }

                foreach (var departamento in usuario.Departamentos)
                {
                    command = new SqlCommand();
                    command.Transaction = transaction;
                    command.Connection = (SqlConnection)_connection;

                    command.CommandText = "INSERT INTO UsuariosDepartamentos (UsuarioId, DepartamentoId) VALUES (@UsuarioId, @DepartamentoId); SELECT CAST(scope_identity() AS int)";
                    command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                    command.Parameters.AddWithValue("@DepartamentoId", departamento.Id);

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    //todo adicionar no log que o rollback falhou

                    return null;

                }

                throw new Exception("ERRO AO TENTAR INSERIR DADOS!!");
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
            SqlTransaction transaction = (SqlTransaction)_connection.BeginTransaction();


            try
            {
                #region Usuario

                SqlCommand command = new SqlCommand();
                command.CommandText = "UPDATE Usuarios SET Nome = @Nome, Email = @Email, Sexo = @Sexo, " +
                                      "RG = @RG, CPF = @CPF, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro," +
                                      "DataCadastro = @DataCadastro WHERE Id = @Id";

                command.Connection = (SqlConnection)_connection;
                command.Transaction = transaction;

                command.Parameters.AddWithValue("@Nome", usuario.Nome);
                command.Parameters.AddWithValue("@Email", usuario.Email);
                command.Parameters.AddWithValue("@Sexo", usuario.Sexo);
                command.Parameters.AddWithValue("@RG", usuario.RG);
                command.Parameters.AddWithValue("@CPF", usuario.CPF);
                command.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
                command.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
                command.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);

                command.Parameters.AddWithValue("@Id", usuario.Id);

                command.ExecuteNonQuery();

                #endregion

                #region Contato
                try
                {
                    command = new SqlCommand();
                    command.Connection = (SqlConnection)_connection;
                    command.Transaction = transaction;

                    command.CommandText = "UPDATE Contatos SET UsuarioId = @UsuarioId, Telefone = @Telefone, Celular = @Celular WHERE Id = @Id";

                    command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                    command.Parameters.AddWithValue("@Telefone", usuario.Contato.Telefone);
                    command.Parameters.AddWithValue("@Celular", usuario.Contato.Celular);

                    command.Parameters.AddWithValue("@Id", usuario.Contato.Id);

                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    throw new Exception("ERRO AO TENTAR ATUALIZAR CONTATO!!");
                }
                #endregion

                #region Enderecos

                try
                {
                    command.CommandText = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @UsuarioId";
                    command.Parameters.Add(usuario.Id);

                    command.ExecuteNonQuery();


                    foreach (var endereco in usuario.EnderecosDeEntrega)
                    {
                        command = new SqlCommand();
                        command.Transaction = transaction;
                        command.Connection = (SqlConnection)_connection;

                        command.CommandText = "INSERT INTO EnderecosEntrega (UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES" +
                            "(@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento)" +
                            "SELECT CAST(scope_identity() AS int)";

                        command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
                        command.Parameters.AddWithValue("@NomeEndereco", endereco.NomeEndereco);
                        command.Parameters.AddWithValue("@CEP", endereco.CEP);
                        command.Parameters.AddWithValue("@Estado", endereco.Estado);
                        command.Parameters.AddWithValue("@Cidade", endereco.Cidade);
                        command.Parameters.AddWithValue("@Bairro", endereco.Bairro);
                        command.Parameters.AddWithValue("@Endereco", endereco.Endereco);
                        command.Parameters.AddWithValue("@Numero", endereco.Numero);
                        command.Parameters.AddWithValue("@Complemento", endereco.Complemento);

                        endereco.Id = (int)command.ExecuteScalar();
                        endereco.UsuarioId = usuario.Id;
                    }
                }
                catch (Exception)
                {
                    throw new Exception("ERRO AO TENTAR ATUALIZAR ENDERECOS!!");
                }

                #endregion

                transaction.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    //todo adicionar no log que o rollback falhou
                }

                throw new Exception("ERRO AO TENTAR ATUALIZAR DADOS!!");
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}
