using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    public partial class TelaPrincipal : Window
    {
        public TelaPrincipal()
        {
            InitializeComponent();

            CarregarDados();
        }

        private void CarregarDados()
        {
            txtNome.Text =
                Sessao.NomeCompleto;

            txtUsuario.Text =
                "@" + Sessao.Usuario;

            txtTipo.Text =
                Sessao.TipoUsuario;

            txtUltimoLogin.Text =
                "Acesso realizado com sucesso.";

            try
            {
                BitmapImage avatar = new BitmapImage();

                avatar.BeginInit();

                avatar.UriSource = new Uri(
                    "Avatares/" + Sessao.Avatar,
                    UriKind.Relative);

                avatar.EndInit();

                // Coloca a imagem dentro do Image do XAML
                imgAvatar.Source = avatar;
            }
            catch
            {
                // Se a imagem não for encontrada
                imgAvatar.Source = null;
            }

            // Usuário comum não acessa auditoria
            btnAuditoria.Visibility =
                Sessao.EhAdministrador
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void Usuarios_Click(
            object sender,
            RoutedEventArgs e)
        {
                TelaUsuarios tela = new TelaUsuarios();

            tela.Show();

            this.Close();
        }

        private void Auditoria_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!Sessao.EhAdministrador)
                return;

            Auditoria tela = new Auditoria();

            tela.Show();

            this.Close();
        }

        private void MeuPerfil_Click(
            object sender,
            RoutedEventArgs e)
        {
            EditarUsuario tela =
                new EditarUsuario(Sessao.Id, false);

            tela.Show();

            this.Close();
        }

        private void Sair_Click(
            object sender,
            RoutedEventArgs e)
        {
            Sessao.Limpar();

            MainWindow login =
                new MainWindow();

            login.Show();

            this.Close();
        }
    }
}