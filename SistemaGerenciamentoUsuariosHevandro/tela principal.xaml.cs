using MySql.Data.MySqlClient;
using System;
using System.IO;
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
            txtNome.Text = Sessao.NomeCompleto;

            txtUsuario.Text =
                "@" + Sessao.Usuario;

            txtTipo.Text =
                Sessao.TipoUsuario;

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

            CarregarAvatar();

            btnAuditoria.Visibility =
                Sessao.EhAdministrador
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }
        // AVATAR

        private void CarregarAvatar()
        {
            try
            {
                string avatar = Sessao.Avatar;

                // Avatar padrão caso não exista na sessão
                if (string.IsNullOrWhiteSpace(avatar) ||
                    avatar.Contains("StackPanel"))
                {
                    avatar = "avatar01.png";
                }

                avatar = avatar.Trim();

                // PASTA DO EXECUTÁVEL

                string pastaAvatares =
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Avatares");

                string caminho =
                    Path.Combine(
                        pastaAvatares,
                        avatar);

                if (File.Exists(caminho))
                {
                    MostrarAvatar(caminho);
                    return;
                }

                // PASTA DO PROJETO

                DirectoryInfo diretorio =
                    Directory.GetParent(
                        AppDomain.CurrentDomain.BaseDirectory);

                if (diretorio != null)
                    diretorio = diretorio.Parent;

                if (diretorio != null)
                    diretorio = diretorio.Parent;

                if (diretorio != null)
                {
                    string caminhoProjeto =
                        Path.Combine(
                            diretorio.FullName,
                            "Avatares",
                            avatar);

                    if (File.Exists(caminhoProjeto))
                    {
                        MostrarAvatar(caminhoProjeto);
                        return;
                    }
                }

                // CAMINHO ALTERNATIVO

                string caminhoAtual =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "Avatares",
                        avatar);

                if (File.Exists(caminhoAtual))
                {
                    MostrarAvatar(caminhoAtual);
                    return;
                }

                // Não mostra mensagem de erro
                imgAvatar.Source = null;
            }
            catch
            {
                // Não mostra mensagem de erro
                imgAvatar.Source = null;
            }
        }

        // MOSTRAR AVATAR

        private void MostrarAvatar(string caminho)
        {
            try
            {
                BitmapImage imagem =
                    new BitmapImage();

                imagem.BeginInit();

                imagem.UriSource =
                    new Uri(
                        caminho,
                        UriKind.Absolute);

                imagem.CacheOption =
                    BitmapCacheOption.OnLoad;

                imagem.CreateOptions =
                    BitmapCreateOptions.IgnoreImageCache;

                imagem.EndInit();

                imagem.Freeze();

                imgAvatar.Source =
                    imagem;
            }
            catch
            {
                imgAvatar.Source = null;
            }
        }

        // USUÁRIOS

        private void Usuarios_Click(
            object sender,
            RoutedEventArgs e)
        {
            TelaUsuarios tela =
                new TelaUsuarios();

            tela.Show();

            Close();
        }

           // AUDITORIA

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

        // MEU PERFIL
 
        private void MeuPerfil_Click(
            object sender,
            RoutedEventArgs e)
        {
            EditarUsuario tela =
                new EditarUsuario(
                    Sessao.Id,
                    Sessao.EhAdministrador);

            tela.Show();

            Close();
        }

        // SAIR

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