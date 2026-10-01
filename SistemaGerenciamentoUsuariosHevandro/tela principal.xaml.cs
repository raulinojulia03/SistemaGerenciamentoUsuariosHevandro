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

              // ÚLTIMO LOGIN
  
            if (string.IsNullOrWhiteSpace(Sessao.UltimoLogin))
            {
                txtUltimoLogin.Text =
                    "Primeiro acesso.";
            }
            else
            {
                txtUltimoLogin.Text =
                    Sessao.UltimoLogin;
            }
            // AVATAR

            try
            {
                BitmapImage avatar =
                    new BitmapImage();

                avatar.BeginInit();

                avatar.UriSource =
                    new Uri(
                        "Avatares/" + Sessao.Avatar,
                        UriKind.Relative);

                avatar.EndInit();

                imgAvatar.Source =
                    avatar;
            }
            catch
            {
                imgAvatar.Source = null;
            }
            // AUDITORIA

            btnAuditoria.Visibility =
                Sessao.EhAdministrador
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void Usuarios_Click(
            object sender,
            RoutedEventArgs e)
        {
            TelaUsuarios tela =
                new TelaUsuarios();

            tela.Show();

            Close();
        }

        private void Auditoria_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!Sessao.EhAdministrador)
                return;

            Auditoria tela =
                new Auditoria();

            tela.Show();

            Close();
        }

        private void MeuPerfil_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Passa a informação real de administrador.
            // Isso permite que EditarUsuario saiba
            // que é o próprio ADM editando a própria conta.

            EditarUsuario tela =
                new EditarUsuario(
                    Sessao.Id,
                    Sessao.EhAdministrador);

            tela.Show();

            Close();
        }

        private void Sair_Click(
            object sender,
            RoutedEventArgs e)
        {
            Sessao.Limpar();

            MainWindow login =
                new MainWindow();

            login.Show();

            Close();
        }
    }
}