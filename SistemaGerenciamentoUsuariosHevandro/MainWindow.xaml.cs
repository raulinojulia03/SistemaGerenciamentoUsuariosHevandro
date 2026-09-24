using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txt_user.Text.Trim();
            string senha = txt_pass.Password.Trim();

            if (string.IsNullOrWhiteSpace(usuario) ||
                string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show(
                    "Usuário e senha são obrigatórios.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                using (MySqlConnection conexao = Banco.CriarConexao())
                {
                    conexao.Open();

                    string query = @"
                        SELECT
                            id,
                            nome_completo,
                            usuario,
                            email,
                            senha,
                            tipo_usuario,
                            status,
                            avatar,
                            ultimo_login,
                            tentativas_login,
                            bloqueado_ate
                        FROM usuarios
                        WHERE usuario = @usuario";

                    using (MySqlCommand comando =
                        new MySqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@usuario", usuario);

                        using (MySqlDataReader reader =
                            comando.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                RegistrarLogin(usuario, "LOGIN_INVALIDO");

                                MessageBox.Show(
                                    "Usuário ou senha inválidos.",
                                    "Erro de login",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);

                                return;
                            }

                            int id = Convert.ToInt32(reader["id"]);

                            string nome = reader["nome_completo"].ToString();
                            string usuarioBanco = reader["usuario"].ToString();
                            string email = reader["email"].ToString();
                            string senhaBanco = reader["senha"].ToString();
                            string tipo = reader["tipo_usuario"].ToString();
                            string status = reader["status"].ToString();
                            string avatar = reader["avatar"].ToString();

                            int tentativas =
                                Convert.ToInt32(reader["tentativas_login"]);

                            DateTime? bloqueadoAte = null;

                            if (reader["bloqueado_ate"] != DBNull.Value)
                            {
                                bloqueadoAte =
                                    Convert.ToDateTime(reader["bloqueado_ate"]);
                            }

                            reader.Close();

                            // Verifica status
                            if (status != "Ativo")
                            {
                                MessageBox.Show(
                                    "Este usuário está inativo.",
                                    "Acesso bloqueado",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            // Verifica bloqueio
                            if (bloqueadoAte != null &&
                                bloqueadoAte > DateTime.Now)
                            {
                                MessageBox.Show(
                                    "Este usuário está temporariamente bloqueado.\n\n" +
                                    "Tente novamente mais tarde.",
                                    "Acesso bloqueado",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            // Verifica senha
                            if (!BCrypt.Net.BCrypt.Verify(
                                    senha,
                                    senhaBanco))
                            {
                                RegistrarTentativaInvalida(
                                    id,
                                    usuarioBanco,
                                    tentativas);

                                return;
                            }

                            // LOGIN CORRETO
                            AtualizarLogin(id);

                            Sessao.Id = id;
                            Sessao.NomeCompleto = nome;
                            Sessao.Usuario = usuarioBanco;
                            Sessao.Email = email;
                            Sessao.TipoUsuario = tipo;
                            Sessao.Status = status;
                            Sessao.Avatar = avatar;

                            RegistrarLogin(
                                usuarioBanco,
                                "LOGIN_SUCESSO");

                            TelaPrincipal tela = new TelaPrincipal();

                            tela.Show();

                            this.Close();
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Erro ao acessar o banco de dados:\n" +
                    ex.Message,
                    "Erro MySQL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocorreu um erro:\n" + ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RegistrarTentativaInvalida(
            int id,
            string usuario,
            int tentativasAtuais)
        {
            int novasTentativas = tentativasAtuais + 1;

            try
            {
                using (MySqlConnection conexao = Banco.CriarConexao())
                {
                    conexao.Open();

                    DateTime? bloqueadoAte = null;

                    // 5 tentativas = bloqueio por 5 minutos
                    if (novasTentativas >= 5)
                    {
                        bloqueadoAte = DateTime.Now.AddMinutes(5);
                        novasTentativas = 0;
                    }

                    string query = @"
                        UPDATE usuarios
                        SET tentativas_login = @tentativas,
                            bloqueado_ate = @bloqueado
                        WHERE id = @id";

                    using (MySqlCommand comando =
                        new MySqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@tentativas",
                            novasTentativas);

                        comando.Parameters.AddWithValue(
                            "@bloqueado",
                            bloqueadoAte);

                        comando.Parameters.AddWithValue(
                            "@id",
                            id);

                        comando.ExecuteNonQuery();
                    }

                    RegistrarLogin(
                        usuario,
                        "LOGIN_INVALIDO");

                    if (bloqueadoAte != null)
                    {
                        RegistrarLogin(
                            usuario,
                            "CONTA_BLOQUEADA");

                        MessageBox.Show(
                            "Foram realizadas várias tentativas inválidas.\n\n" +
                            "O usuário foi bloqueado temporariamente por 5 minutos.",
                            "Conta bloqueada",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Usuário ou senha inválidos.",
                            "Erro de login",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao registrar tentativa:\n" +
                    ex.Message);
            }
        }

        private void AtualizarLogin(int id)
        {
            using (MySqlConnection conexao = Banco.CriarConexao())
            {
                conexao.Open();

                string query = @"
                    UPDATE usuarios
                    SET ultimo_login = NOW(),
                        tentativas_login = 0,
                        bloqueado_ate = NULL
                    WHERE id = @id";

                using (MySqlCommand comando =
                    new MySqlCommand(query, conexao))
                {
                    comando.Parameters.AddWithValue("@id", id);

                    comando.ExecuteNonQuery();
                }
            }
        }

        private void RegistrarLogin(
            string usuario,
            string evento)
        {
            try
            {
                using (MySqlConnection conexao = Banco.CriarConexao())
                {
                    conexao.Open();

                    string query = @"
                        INSERT INTO log_autenticacao
                        (usuario, evento, resultado)
                        VALUES
                        (@usuario, @evento, @resultado)";

                    using (MySqlCommand comando =
                        new MySqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@usuario", usuario);

                        comando.Parameters.AddWithValue(
                            "@evento", evento);

                        comando.Parameters.AddWithValue(
                            "@resultado",
                            evento == "LOGIN_SUCESSO"
                                ? "SUCESSO"
                                : "FALHA");

                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Não interrompe o login por erro no log.
            }
        }

        private void Cadastro_Click_1(
            object sender,
            RoutedEventArgs e)
        {
            // Cadastro de usuários será exclusivo
            // para administradores.
            MessageBox.Show(
                "O cadastro de novos usuários deve ser realizado por um administrador.",
                "Acesso restrito",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}