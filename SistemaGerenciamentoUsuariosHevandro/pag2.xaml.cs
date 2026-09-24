using MySql.Data.MySqlClient;
using System;
using System.Windows;
using BCryptNet = BCrypt.Net.BCrypt;

namespace WpfApp1
{
    public partial class CadastroUsuario : Window
    {
        private readonly string connectionString =
            "Server=localhost;Database=login;Uid=root;Pwd=;";

        public CadastroUsuario()
        {
            InitializeComponent();

            // Valores iniciais dos campos
            cmbTipo.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbAvatar.SelectedIndex = 0;
        }

        // =========================================================
        // BOTÃO CADASTRAR
        // =========================================================
        private void Cadastrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            string nome = txtNome.Text.Trim();
            string usuario = txtUsuario.Text.Trim();
            string email = txtEmail.Text.Trim();
            string senha = txtSenha.Password.Trim();
            string confirmarSenha = txtConfirmarSenha.Password.Trim();

            string tipoUsuario = "Usuario";
            string status = "Ativo";
            string avatar = "avatar01.png";

            // Tipo selecionado
            if (cmbTipo.SelectedItem is System.Windows.Controls.ComboBoxItem tipo)
            {
                tipoUsuario = tipo.Content.ToString();
            }

            // Status selecionado
            if (cmbStatus.SelectedItem is System.Windows.Controls.ComboBoxItem statusItem)
            {
                status = statusItem.Content.ToString();
            }

            // Avatar selecionado
            if (cmbAvatar.SelectedItem is System.Windows.Controls.ComboBoxItem avatarItem)
            {
                avatar = avatarItem.Content.ToString();
            }

            // =====================================================
            // VALIDAÇÕES
            // =====================================================

            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(usuario) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha) ||
                string.IsNullOrWhiteSpace(confirmarSenha))
            {
                MessageBox.Show(
                    "Preencha todos os campos obrigatórios.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!email.Contains("@"))
            {
                MessageBox.Show(
                    "Digite um e-mail válido.",
                    "E-mail inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (usuario.Length < 3)
            {
                MessageBox.Show(
                    "O usuário deve ter no mínimo 3 caracteres.",
                    "Usuário inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (senha.Length < 8)
            {
                MessageBox.Show(
                    "A senha deve ter no mínimo 8 caracteres.",
                    "Senha inválida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (senha != confirmarSenha)
            {
                MessageBox.Show(
                    "As senhas não coincidem.",
                    "Senha inválida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    // =================================================
                    // VERIFICA SE O USUÁRIO JÁ EXISTE
                    // =================================================

                    string verificarUsuario = @"
                        SELECT COUNT(*)
                        FROM usuarios
                        WHERE usuario = @usuario";

                    using (MySqlCommand comandoVerificar =
                        new MySqlCommand(
                            verificarUsuario,
                            conexao))
                    {
                        comandoVerificar.Parameters.AddWithValue(
                            "@usuario",
                            usuario);

                        long quantidade =
                            Convert.ToInt64(
                                comandoVerificar.ExecuteScalar());

                        if (quantidade > 0)
                        {
                            MessageBox.Show(
                                "Esse usuário já está cadastrado.",
                                "Usuário existente",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }
                    }

                    // =================================================
                    // CRIPTOGRAFA A SENHA
                    // =================================================

                    string senhaCriptografada =
                        BCryptNet.HashPassword(senha);

                    // =================================================
                    // CADASTRA NO BANCO
                    // =================================================
                    //
                    // Sua tabela atual possui:
                    // id
                    // email
                    // usuario
                    // senha
                    // isADM
                    //
                    // Como o XAML atual não possui checkbox de
                    // administrador, o novo usuário será criado
                    // como usuário comum (isADM = 0).
                    //

                    string query = @"
                        INSERT INTO usuarios
                        (
                            email,
                            usuario,
                            senha,
                            isADM
                        )
                        VALUES
                        (
                            @email,
                            @usuario,
                            @senha,
                            @isADM
                        )";

                    using (MySqlCommand comando =
                        new MySqlCommand(
                            query,
                            conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@email",
                            email);

                        comando.Parameters.AddWithValue(
                            "@usuario",
                            usuario);

                        comando.Parameters.AddWithValue(
                            "@senha",
                            senhaCriptografada);

                        comando.Parameters.AddWithValue(
                            "@isADM",
                            0);

                        comando.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Usuário cadastrado com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Volta para o login
                MainWindow login = new MainWindow();

                login.Show();

                Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Erro ao acessar o banco de dados:\n\n" +
                    ex.Message,
                    "Erro MySQL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocorreu um erro:\n\n" +
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // BOTÃO CANCELAR
        // =========================================================

        private void Cancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();

            login.Show();

            Close();
        }
    }
}