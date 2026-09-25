using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
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
        private void Cadastrar_Click(object sender, RoutedEventArgs e)
        {
            string nome = txtNome.Text.Trim();
            string usuario = txtUsuario.Text.Trim();
            string email = txtEmail.Text.Trim();
            string senha = txtSenha.Password;
            string confirmarSenha = txtConfirmarSenha.Password;

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

            if (senha != confirmarSenha)
            {
                MessageBox.Show(
                    "As senhas não coincidem.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (senha.Length < 8)
            {
                MessageBox.Show(
                    "A senha deve possuir pelo menos 8 caracteres.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (cmbTipo.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecione o tipo de usuário.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecione o status.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (cmbAvatar.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecione um avatar.",
                    "Atenção",
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

                    string verificar = @"
                SELECT COUNT(*)
                FROM usuarios
                WHERE usuario = @usuario
                   OR email = @email";

                    using (MySqlCommand comandoVerificar =
                        new MySqlCommand(verificar, conexao))
                    {
                        comandoVerificar.Parameters.AddWithValue(
                            "@usuario",
                            usuario);

                        comandoVerificar.Parameters.AddWithValue(
                            "@email",
                            email);

                        int quantidade =
                            Convert.ToInt32(
                                comandoVerificar.ExecuteScalar());

                        if (quantidade > 0)
                        {
                            MessageBox.Show(
                                "O usuário ou e-mail já está cadastrado.",
                                "Cadastro existente",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }
                    }

                    string senhaCriptografada =
                        BCryptNet.HashPassword(senha);

                    string tipoUsuario =
                        ((ComboBoxItem)cmbTipo.SelectedItem)
                        .Content.ToString();

                    string status =
                        ((ComboBoxItem)cmbStatus.SelectedItem)
                        .Content.ToString();

                    string avatar =
                        ((ComboBoxItem)cmbAvatar.SelectedItem)
                        .Content.ToString();

                    string query = @"
                INSERT INTO usuarios
                (
                    nome_completo,
                    email,
                    usuario,
                    senha,
                    tipo_usuario,
                    status,
                    avatar
                )
                VALUES
                (
                    @nome_completo,
                    @email,
                    @usuario,
                    @senha,
                    @tipo_usuario,
                    @status,
                    @avatar
                )";

                    using (MySqlCommand comando =
                        new MySqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@nome_completo",
                            nome);

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
                            "@tipo_usuario",
                            tipoUsuario);

                        comando.Parameters.AddWithValue(
                            "@status",
                            status);

                        comando.Parameters.AddWithValue(
                            "@avatar",
                            avatar);

                        comando.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Usuário cadastrado com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

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