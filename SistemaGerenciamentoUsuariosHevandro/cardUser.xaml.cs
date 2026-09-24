using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;

namespace WpfApp1
{
    public partial class CardUsuario : UserControl
    {
        public Usuario DadosUsuario { get; set; }

        public CardUsuario(
            Usuario usuario,
            bool administrador)
        {
            InitializeComponent();

            DadosUsuario = usuario;

            CarregarDados(administrador);
        }

        private void CarregarDados(bool administrador)
        {
            txtNome.Text =
                DadosUsuario.NomeCompleto;

            txtUsuario.Text =
                "@" + DadosUsuario.UsuarioNome;

            txtEmail.Text =
                DadosUsuario.Email;

            txtTipo.Text =
                DadosUsuario.TipoUsuario;

            txtStatus.Text =
                DadosUsuario.Status;

            txtUltimoLogin.Text =
                DadosUsuario.UltimoLoginTexto;

            try
            {
                imgAvatar.Source =
                    new BitmapImage(
                        new Uri(
                            "Avatares/" +
                            DadosUsuario.Avatar,
                            UriKind.Relative));
            }
            catch
            {
                imgAvatar.Source = null;
            }

            if (!administrador)
            {
                btnEditar.Visibility =
                    Visibility.Collapsed;

                btnSenha.Visibility =
                    Visibility.Collapsed;

                btnExcluir.Visibility =
                    Visibility.Collapsed;
            }
        }

        private void Editar_Click(
            object sender,
            RoutedEventArgs e)
        {
            EditarUsuario tela =
                new EditarUsuario(
                    DadosUsuario.Id,
                    true);

            tela.Show();

            Window janela =
                Window.GetWindow(this);

            janela?.Close();
        }

        private void Excluir_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DadosUsuario.Id == Sessao.Id)
            {
                MessageBox.Show(
                    "Você não pode excluir sua própria conta enquanto estiver conectado.",
                    "Operação não permitida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult resultado =
                MessageBox.Show(
                    "Deseja realmente excluir o usuário:\n\n" +
                    DadosUsuario.NomeCompleto + "?",
                    "Confirmar exclusão",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            try
            {
                using (var conexao = Banco.CriarConexao())
                {
                    conexao.Open();

                    // Verifica se é o último administrador
                    if (DadosUsuario.TipoUsuario ==
                        "Administrador")
                    {
                        string verificar =
                            "SELECT COUNT(*) FROM usuarios " +
                            "WHERE tipo_usuario = 'Administrador' " +
                            "AND status = 'Ativo'";

                        using (var comando =
                            new MySqlCommand(
                                verificar,
                                conexao))
                        {
                            int quantidade =
                                Convert.ToInt32(
                                    comando.ExecuteScalar());

                            if (quantidade <= 1)
                            {
                                MessageBox.Show(
                                    "Não é possível excluir o último administrador ativo.",
                                    "Operação não permitida",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }
                        }
                    }

                    string query =
                        "DELETE FROM usuarios WHERE id = @id";

                    using (var comando =
                        new MySqlCommand(
                            query,
                            conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@id",
                            DadosUsuario.Id);

                        comando.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Usuário excluído com sucesso.",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                TelaUsuarios tela =
                    new TelaUsuarios();

                tela.Show();

                Window janela =
                    Window.GetWindow(this);

                janela?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao excluir usuário:\n" +
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Senha_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!Sessao.EhAdministrador)
                return;

            RedefinirSenha tela =
                new RedefinirSenha(
                    DadosUsuario.Id);

            tela.ShowDialog();
        }
    }
}