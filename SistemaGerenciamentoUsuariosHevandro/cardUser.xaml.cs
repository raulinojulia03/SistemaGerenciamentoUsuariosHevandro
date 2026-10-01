using System;
using System.IO;
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

            // AVATAR

            CarregarAvatar();

            // Somente administradores podem
            // editar, redefinir senha ou excluir.
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

        // CARREGAR AVATAR

        private void CarregarAvatar()
        {
            try
            {
                string avatar =
                    DadosUsuario.Avatar;

                // Avatar padrão
                if (string.IsNullOrWhiteSpace(avatar) ||
                    avatar.Contains("StackPanel"))
                {
                    avatar = "avatar01.png";
                }

                avatar = avatar.Trim();

                // PRIMEIRA TENTATIVA
                // PASTA DO EXECUTÁVEL

                string caminho =
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Avatares",
                        avatar);

                if (File.Exists(caminho))
                {
                    MostrarAvatar(caminho);
                    return;
                }

                // SEGUNDA TENTATIVA
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
                // TERCEIRA TENTATIVA
                // DIRETÓRIO ATUAL
 
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

                // Se não encontrar, não mostra mensagem.
                imgAvatar.Source = null;
            }
            catch
            {
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

        // EDITAR
 
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

        // EXCLUIR
        private void Excluir_Click(
            object sender,
            RoutedEventArgs e)
        {
            // NÃO PERMITIR QUE O ADMIN EXCLUA A PRÓPRIA CONTA

            if (DadosUsuario.Id == Sessao.Id)
            {
                MessageBox.Show(
                    "Você não pode excluir sua própria conta enquanto estiver conectado.",
                    "Operação não permitida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // CONFIRMAÇÃO DA EXCLUSÃO

            MessageBoxResult resultado =
                MessageBox.Show(
                    "Deseja realmente excluir o usuário:\n\n" +
                    "Nome: " +
                    DadosUsuario.NomeCompleto +
                    "\nUsuário: @" +
                    DadosUsuario.UsuarioNome +
                    "\nE-mail: " +
                    DadosUsuario.Email,
                    "Confirmar exclusão",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (MySqlConnection conexao =
                    Banco.CriarConexao())
                {
                    conexao.Open();

                    // INICIA TRANSAÇÃO

                    using (MySqlTransaction transacao =
                        conexao.BeginTransaction())
                    {
                        try
                        {
                            // VERIFICA SE O USUÁRIO É ADMINISTRADOR
                            // E SE É O ÚLTIMO ADMINISTRADOR

                            if (DadosUsuario.TipoUsuario ==
                                "Administrador")
                            {
                                string verificar =
                                    @"SELECT COUNT(*)
                                      FROM usuarios
                                      WHERE tipo_usuario = 'Administrador'";

                                using (MySqlCommand comando =
                                    new MySqlCommand(
                                        verificar,
                                        conexao,
                                        transacao))
                                {
                                    int quantidade =
                                        Convert.ToInt32(
                                            comando.ExecuteScalar());

                                    if (quantidade <= 1)
                                    {
                                        MessageBox.Show(
                                            "Não é possível excluir o último administrador do sistema.",
                                            "Operação não permitida",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Warning);

                                        transacao.Rollback();
                                        return;
                                    }
                                }
                            }

                            // REGISTRO DA AUDITORIA

                            string valorAnterior =
                                "Nome=" +
                                DadosUsuario.NomeCompleto +
                                " | Usuário=" +
                                DadosUsuario.UsuarioNome +
                                " | E-mail=" +
                                DadosUsuario.Email +
                                " | Perfil=" +
                                DadosUsuario.TipoUsuario +
                                " | Status=" +
                                DadosUsuario.Status +
                                " | Avatar=" +
                                DadosUsuario.Avatar;

                            string auditoria =
                                @"INSERT INTO auditoria
                                (
                                    data_hora,
                                    usuario_responsavel,
                                    operacao,
                                    registro_afetado,
                                    valor_anterior,
                                    novo_valor
                                )
                                VALUES
                                (
                                    NOW(),
                                    @responsavel,
                                    'EXCLUSAO',
                                    @registro,
                                    @valorAnterior,
                                    @novoValor
                                )";

                            using (MySqlCommand comandoAuditoria =
                                new MySqlCommand(
                                    auditoria,
                                    conexao,
                                    transacao))
                            {
                                comandoAuditoria.Parameters.AddWithValue(
                                    "@responsavel",
                                    Sessao.Usuario);

                                comandoAuditoria.Parameters.AddWithValue(
                                    "@registro",
                                    DadosUsuario.UsuarioNome);

                                comandoAuditoria.Parameters.AddWithValue(
                                    "@valorAnterior",
                                    valorAnterior);

                                comandoAuditoria.Parameters.AddWithValue(
                                    "@novoValor",
                                    "—");

                                comandoAuditoria.ExecuteNonQuery();
                            }

                            // EXCLUI O USUÁRIO

                            string query =
                                @"DELETE FROM usuarios
                                  WHERE id = @id";

                            using (MySqlCommand comando =
                                new MySqlCommand(
                                    query,
                                    conexao,
                                    transacao))
                            {
                                comando.Parameters.AddWithValue(
                                    "@id",
                                    DadosUsuario.Id);

                                int registrosAfetados =
                                    comando.ExecuteNonQuery();

                                if (registrosAfetados == 0)
                                {
                                    throw new Exception(
                                        "O usuário não foi encontrado.");
                                }
                            }

                            transacao.Commit();
                        }
                        catch
                        {
                            transacao.Rollback();
                            throw;
                        }
                    }
                }

                // SUCESSO

                MessageBox.Show(
                    "Usuário excluído com sucesso.\n\n" +
                    "A exclusão também foi registrada na auditoria.",
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
                    "Erro ao excluir usuário:\n\n" +
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // REDEFINIR SENHA

        private void Senha_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!Sessao.EhAdministrador)
            {
                return;
            }

            RedefinirSenha tela =
                new RedefinirSenha(
                    DadosUsuario.Id);

            tela.ShowDialog();
        }
    }
}