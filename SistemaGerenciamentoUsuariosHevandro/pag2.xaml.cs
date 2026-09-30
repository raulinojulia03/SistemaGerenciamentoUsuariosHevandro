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

        private bool administradorCadastrando;

        public CadastroUsuario()
            : this(Sessao.EhAdministrador)
        {
        }

        public CadastroUsuario(bool administrador)
        {
            InitializeComponent();

            administradorCadastrando = administrador;

            cmbTipo.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbAvatar.SelectedIndex = 0;

            VerificarPrimeiroCadastro();
        }

        private void VerificarPrimeiroCadastro()
        {
            try
            {
                using (MySqlConnection conexao =
                    new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string query =
                        "SELECT COUNT(*) FROM usuarios";

                    using (MySqlCommand comando =
                        new MySqlCommand(query, conexao))
                    {
                        int quantidade =
                            Convert.ToInt32(
                                comando.ExecuteScalar());

                        // PRIMEIRO USUÁRIO
                        if (quantidade == 0)
                        {
                            cmbTipo.SelectedIndex = 1;
                            cmbTipo.IsEnabled = false;

                            MessageBox.Show(
                                "Este é o primeiro cadastro do sistema.\n\n" +
                                "O primeiro usuário será cadastrado automaticamente " +
                                "como Administrador.",
                                "Primeiro cadastro",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            // Se foi aberto por administrador,
                            // ele pode escolher o tipo.
                            if (administradorCadastrando)
                            {
                                cmbTipo.IsEnabled = true;
                            }
                            else
                            {
                                // Cadastro comum sempre será Usuario
                                cmbTipo.SelectedIndex = 0;
                                cmbTipo.IsEnabled = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao verificar os usuários cadastrados:\n\n" +
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cadastrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            string nome =
                txtNome.Text.Trim();

            string usuario =
                txtUsuario.Text.Trim();

            string email =
                txtEmail.Text.Trim();

            string senha =
                txtSenha.Password;

            string confirmarSenha =
                txtConfirmarSenha.Password;

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

                    // VERIFICA SE USUÁRIO OU E-MAIL JÁ EXISTE

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

                    // VERIFICA QUANTOS USUÁRIOS JÁ EXISTEM

                    string verificarPrimeiro = @"
                        SELECT COUNT(*)
                        FROM usuarios";

                    int totalUsuarios;

                    using (MySqlCommand comandoPrimeiro =
                        new MySqlCommand(
                            verificarPrimeiro,
                            conexao))
                    {
                        totalUsuarios =
                            Convert.ToInt32(
                                comandoPrimeiro.ExecuteScalar());
                    }

                    // DEFINE O TIPO DO USUÁRIO

                    string tipoUsuario;

                    if (totalUsuarios == 0)
                    {
                        // Primeiro usuário obrigatoriamente é administrador
                        tipoUsuario = "Administrador";
                    }
                    else if (administradorCadastrando)
                    {
                        // Administrador pode escolher o tipo
                        tipoUsuario =
                            ((ComboBoxItem)cmbTipo.SelectedItem)
                            .Content.ToString();
                    }
                    else
                    {
                        // Usuário comum só pode criar usuário comum
                        tipoUsuario = "Usuario";
                    }

                    string status =
                        ((ComboBoxItem)cmbStatus.SelectedItem)
                        .Content.ToString();

                    string avatar =
                        ((ComboBoxItem)cmbAvatar.SelectedItem)
                        .Content.ToString();

                    string senhaCriptografada =
                        BCryptNet.HashPassword(senha);

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

                    if (tipoUsuario == "Administrador")
                    {
                        MessageBox.Show(
                            "Primeiro usuário cadastrado com sucesso!\n\n" +
                            "Este usuário foi definido automaticamente " +
                            "como Administrador.",
                            "Cadastro concluído",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Usuário cadastrado com sucesso!",
                            "Cadastro concluído",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }

                MainWindow login =
                    new MainWindow();

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

        private void Cancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            MainWindow login =
                new MainWindow();

            login.Show();

            Close();
        }
    }
}