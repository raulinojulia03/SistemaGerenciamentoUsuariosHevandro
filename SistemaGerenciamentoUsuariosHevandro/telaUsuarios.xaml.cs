using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
                        new MySqlCommand(
                            query,
                            conexao))
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

                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao carregar usuários:\n\n" +
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        // APLICAR FILTROS

        private void AplicarFiltros()
        {
            if (painelCards == null ||
                cmbStatusFiltro == null ||
                cmbTipoFiltro == null ||
                txtPesquisa == null)
            {
                return;
            }

            string pesquisa =
                txtPesquisa.Text
                .Trim()
                .ToLower();

            string statusSelecionado = "";
            string tipoSelecionado = "";

            // STATUS

            if (cmbStatusFiltro.SelectedItem
                is ComboBoxItem itemStatus)
            {
                string texto =
                    itemStatus.Content?.ToString();

                if (texto == "Status: Ativo")
                {
                    statusSelecionado = "Ativo";
                }
                else if (texto == "Status: Inativo")
                {
                    statusSelecionado = "Inativo";
                }
            }

            // PERFIL

            if (cmbTipoFiltro.SelectedItem
                is ComboBoxItem itemTipo)
            {
                string texto =
                    itemTipo.Content?.ToString();

                if (texto == "Perfil: Usuário")
                {
                    tipoSelecionado = "Usuario";
                }
                else if (texto == "Perfil: Administrador")
                {
                    tipoSelecionado = "Administrador";
                }
            }

            // FILTRAR-------

            List<Usuario> filtrados =
                usuarios.FindAll(u =>
                {
                    bool correspondePesquisa =
                        string.IsNullOrWhiteSpace(pesquisa)
                        ||
                        (!string.IsNullOrWhiteSpace(
                            u.NomeCompleto) &&
                         u.NomeCompleto
                            .ToLower()
                            .Contains(pesquisa))
                        ||
                        (!string.IsNullOrWhiteSpace(
                            u.UsuarioNome) &&
                         u.UsuarioNome
                            .ToLower()
                            .Contains(pesquisa))
                        ||
                        (!string.IsNullOrWhiteSpace(
                            u.Email) &&
                         u.Email
                            .ToLower()
                            .Contains(pesquisa));

                    bool correspondeStatus =
                        string.IsNullOrWhiteSpace(
                            statusSelecionado)
                        ||
                        u.Status ==
                            statusSelecionado;

                    bool correspondeTipo =
                        string.IsNullOrWhiteSpace(
                            tipoSelecionado)
                        ||
                        u.TipoUsuario ==
                            tipoSelecionado;

                    return
                        correspondePesquisa &&
                        correspondeStatus &&
                        correspondeTipo;
                });

            ExibirCards(filtrados);
        }

        // EXIBIR CARDS

        private void ExibirCards(
            List<Usuario> lista)
        {
            if (painelCards == null)
            {
                return;
            }

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


        // PESQUISA
        private void txtPesquisa_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (painelCards == null ||
                cmbStatusFiltro == null ||
                cmbTipoFiltro == null)
            {
                return;
            }

            AplicarFiltros();
        }

        // FILTROS---------

        private void Filtro_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (painelCards == null ||
                cmbStatusFiltro == null ||
                cmbTipoFiltro == null)
            {
                return;
            }

            AplicarFiltros();
        }

        // NOVO USUÁRIO

        private void NovoUsuario_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Somente administrador pode cadastrar
            if (!Sessao.EhAdministrador)
            {
                MessageBox.Show(
                    "Somente administradores podem cadastrar usuários.",
                    "Acesso negado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // true informa que o cadastro foi aberto
            // pelo administrador.
            CadastroUsuario tela =
                new CadastroUsuario(true);

            tela.Show();

            Close();
        }
        private void Voltar_Click(
            object sender,
            RoutedEventArgs e)
        {
            TelaPrincipal tela =
                new TelaPrincipal();

            tela.Show();

            Close();
        }
    }
}