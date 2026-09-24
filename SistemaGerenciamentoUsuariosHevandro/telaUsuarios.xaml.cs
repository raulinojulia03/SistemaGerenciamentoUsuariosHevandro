using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfApp1
{
    public partial class TelaUsuarios : Window
    {
        private List<Usuario> usuarios =
            new List<Usuario>();

        public TelaUsuarios()
        {
            InitializeComponent();

            CarregarUsuarios();
        }

        private void CarregarUsuarios()
        {
            usuarios.Clear();

            try
            {
                using (MySqlConnection conexao =
                    Banco.CriarConexao())
                {
                    conexao.Open();

                    string query = @"
                        SELECT
                            id,
                            nome_completo,
                            usuario,
                            email,
                            tipo_usuario,
                            status,
                            avatar,
                            data_criacao,
                            data_alteracao,
                            ultimo_login
                        FROM usuarios
                        ORDER BY nome_completo";

                    using (MySqlCommand comando =
                        new MySqlCommand(query, conexao))
                    {
                        using (MySqlDataReader reader =
                            comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Usuario usuario =
                                    new Usuario();

                                usuario.Id =
                                    Convert.ToInt32(
                                        reader["id"]);

                                usuario.NomeCompleto =
                                    reader["nome_completo"]
                                    .ToString();

                                usuario.UsuarioNome =
                                    reader["usuario"]
                                    .ToString();

                                usuario.Email =
                                    reader["email"]
                                    .ToString();

                                usuario.TipoUsuario =
                                    reader["tipo_usuario"]
                                    .ToString();

                                usuario.Status =
                                    reader["status"]
                                    .ToString();

                                usuario.Avatar =
                                    reader["avatar"]
                                    .ToString();

                                usuario.DataCriacao =
                                    Convert.ToDateTime(
                                        reader["data_criacao"]);

                                if (reader["data_alteracao"] !=
                                    DBNull.Value)
                                {
                                    usuario.DataAlteracao =
                                        Convert.ToDateTime(
                                            reader["data_alteracao"]);
                                }

                                if (reader["ultimo_login"] !=
                                    DBNull.Value)
                                {
                                    usuario.UltimoLogin =
                                        Convert.ToDateTime(
                                            reader["ultimo_login"]);
                                }

                                usuarios.Add(usuario);
                            }
                        }
                    }
                }

                ExibirCards(usuarios);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao carregar usuários:\n" +
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExibirCards(
            List<Usuario> lista)
        {
            painelCards.Children.Clear();

            foreach (Usuario usuario in lista)
            {
                CardUsuario card =
                    new CardUsuario(
                        usuario,
                        Sessao.EhAdministrador);

                painelCards.Children.Add(card);
            }
        }

        private void txtPesquisa_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e)
        {
            string pesquisa =
                txtPesquisa.Text.Trim()
                .ToLower();

            if (string.IsNullOrWhiteSpace(pesquisa))
            {
                ExibirCards(usuarios);
                return;
            }

            List<Usuario> filtrados =
                usuarios.FindAll(u =>
                    u.NomeCompleto
                        .ToLower()
                        .Contains(pesquisa)
                    ||
                    u.UsuarioNome
                        .ToLower()
                        .Contains(pesquisa)
                    ||
                    u.Email
                        .ToLower()
                        .Contains(pesquisa));

            ExibirCards(filtrados);
        }

        private void NovoUsuario_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!Sessao.EhAdministrador)
            {
                MessageBox.Show(
                    "Somente administradores podem cadastrar usuários.",
                    "Acesso negado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            CadastroUsuario tela =
                new CadastroUsuario();

            tela.Show();

            this.Close();
        }

        private void Voltar_Click(
            object sender,
            RoutedEventArgs e)
        {
            TelaPrincipal tela =
                new TelaPrincipal();

            tela.Show();

            this.Close();
        }
    }
}