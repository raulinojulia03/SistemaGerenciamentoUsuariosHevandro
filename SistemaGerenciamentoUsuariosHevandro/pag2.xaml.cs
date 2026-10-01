using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using BCryptNet = BCrypt.Net.BCrypt;

namespace WpfApp1
{
    public partial class CadastroUsuario : Window
    {
        private bool administradorCadastrando;
        private bool primeiroCadastro;

        public CadastroUsuario()
            : this(Sessao.EhAdministrador)
        {
        }

        public CadastroUsuario(bool administrador)
        {
            InitializeComponent();

            administradorCadastrando = administrador;

            // Valores iniciais
            cmbTipo.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            cmbAvatar.SelectedIndex = 0;

            VerificarPrimeiroCadastro();
        }

        // VERIFICA PRIMEIRO CADASTRO

        private void VerificarPrimeiroCadastro()
        {
            try
            {
                using (MySqlConnection conexao = Banco.CriarConexao())
                {
                    conexao.Open();

                    string query = @"
                        SELECT COUNT(*)
                        FROM usuarios";

                    using (MySqlCommand comando =
                        new MySqlCommand(query, conexao))
                    {
                        int quantidade =
                            Convert.ToInt32(
                                comando.ExecuteScalar());

                        // PRIMEIRO USUÁRIO

                        if (quantidade == 0)
                        {
                            primeiroCadastro = true;

                            // Primeiro usuário será Administrador
                            cmbTipo.SelectedIndex = 1;
                            cmbTipo.IsEnabled = false;

                            // Primeiro usuário será Ativo.
                            // Não será necessário escolher o status.
                            cmbStatus.SelectedIndex = -1;
                            cmbStatus.IsEnabled = false;

                            MessageBox.Show(
                                "Este é o primeiro cadastro do sistema.\n\n" +
                                "O primeiro usuário será cadastrado " +
                                "automaticamente como Administrador e Ativo.",
                                "Primeiro cadastro",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            primeiroCadastro = false;

                            // ADMINISTRADOR CADASTRANDO

                            if (administradorCadastrando)
                            {
                                cmbTipo.IsEnabled = true;
                                cmbStatus.IsEnabled = true;

                                cmbTipo.SelectedIndex = 0;
                                cmbStatus.SelectedIndex = 0;
                            }

                            // OUTRA SITUAÇÃO

                            else
                            {
                                // Cadastro comum sempre será Usuário
                                cmbTipo.SelectedIndex = 0;
                                cmbTipo.IsEnabled = false;

                                // Usuário comum sempre será Ativo
                                cmbStatus.SelectedIndex = 0;
                                cmbStatus.IsEnabled = false;
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

        // CADASTRAR
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

            // VALIDAÇÕES

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

            // USUÁRIO
 
            if (usuario.Length < 3)
            {
                MessageBox.Show(
                    "O usuário deve possuir pelo menos 3 caracteres.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtUsuario.Focus();
                return;
            }

            // E-MAIL

            if (!email.Contains("@") ||
                !email.Contains(".") ||
                email.StartsWith("@") ||
                email.EndsWith("@") ||
                email.StartsWith(".") ||
                email.EndsWith("."))
            {
                MessageBox.Show(
                    "Informe um e-mail válido.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtEmail.Focus();
                return;
            }

            // SENHA

            if (senha.Length < 8)
            {
                MessageBox.Show(
                    "A senha deve possuir pelo menos 8 caracteres.",
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

            // STATUS

            if (!primeiroCadastro &&
                cmbStatus.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecione o status.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // AVATAR

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
                    Banco.CriarConexao())
                {
                    conexao.Open();

                    // VERIFICA DUPLICIDADE

                    string verificar = @"
                        SELECT COUNT(*)
                        FROM usuarios
                        WHERE usuario = @usuario
                           OR email = @email";

                    using (MySqlCommand comandoVerificar =
                        new MySqlCommand(
                            verificar,
                            conexao))
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

                    // VERIFICA NOVAMENTE SE É O PRIMEIRO CADASTRO

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

                    if (totalUsuarios == 0)
                    {
                        primeiroCadastro = true;
                    }
                    else
                    {
                        primeiroCadastro = false;
                    }

                    // DEFINE TIPO DO USUÁRIO

                    string tipoUsuario;

                    if (primeiroCadastro)
                    {
                        // Primeiro usuário sempre é Administrador
                        tipoUsuario = "Administrador";
                    }
                    else if (administradorCadastrando)
                    {
                        if (cmbTipo.SelectedItem == null)
                        {
                            MessageBox.Show(
                                "Selecione o tipo de usuário.",
                                "Atenção",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        tipoUsuario =
                            ((ComboBoxItem)cmbTipo.SelectedItem)
                            .Content
                            .ToString();
                    }
                    else
                    {
                        // Cadastro que não é feito pelo administrador
                        tipoUsuario = "Usuario";
                    }

                    // DEFINE AVATAR

                    string avatar =
                        ((ComboBoxItem)cmbAvatar.SelectedItem)
                        .Content
                        .ToString();

                    // DEFINE STATUS

                    string status;

                    if (primeiroCadastro)
                    {
                        // O banco também utiliza Ativo como padrão,
                        // mas definimos aqui para a auditoria.
                        status = "Ativo";
                    }
                    else
                    {
                        status =
                            ((ComboBoxItem)cmbStatus.SelectedItem)
                            .Content
                            .ToString();
                    }

                    // CRIPTOGRAFA SENHA

                    string senhaCriptografada =
                        BCryptNet.HashPassword(senha);

                    // INICIA TRANSAÇÃO

                    using (MySqlTransaction transacao =
                        conexao.BeginTransaction())
                    {
                        try
                        {
                            // CADASTRA USUÁRIO

                            string query;

                            // Primeiro cadastro
                            if (primeiroCadastro)
                            {
                                query = @"
                                    INSERT INTO usuarios
                                    (
                                        nome_completo,
                                        email,
                                        usuario,
                                        senha,
                                        tipo_usuario,
                                        avatar
                                    )
                                    VALUES
                                    (
                                        @nome_completo,
                                        @email,
                                        @usuario,
                                        @senha,
                                        @tipo_usuario,
                                        @avatar
                                    )";
                            }
                            else
                            {
                                query = @"
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
                            }

                            using (MySqlCommand comando =
                                new MySqlCommand(
                                    query,
                                    conexao,
                                    transacao))
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

                                if (!primeiroCadastro)
                                {
                                    comando.Parameters.AddWithValue(
                                        "@status",
                                        status);
                                }

                                comando.Parameters.AddWithValue(
                                    "@avatar",
                                    avatar);

                                comando.ExecuteNonQuery();
                            }

                            // AUDITORIA DO CADASTRO

                            string valorAnterior =
                                "—";

                            string novoValor =
                                "Nome=" +
                                nome +
                                " | Usuário=" +
                                usuario +
                                " | E-mail=" +
                                email +
                                " | Perfil=" +
                                tipoUsuario +
                                " | Status=" +
                                status +
                                " | Avatar=" +
                                avatar;

                            // RESPONSÁVEL PELO CADASTRO
                            // Se for o primeiro cadastro, ainda não existe
                            // uma sessão autenticada. Nesse caso usamos
                            // SISTEMA como responsável.

                            string responsavel;

                            if (primeiroCadastro ||
                                string.IsNullOrWhiteSpace(
                                    Sessao.Usuario))
                            {
                                responsavel = "SISTEMA";
                            }
                            else
                            {
                                responsavel =
                                    Sessao.Usuario;
                            }

                            string auditoria = @"
                                INSERT INTO auditoria
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
                                    'CADASTRO',
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
                                    responsavel);

                                comandoAuditoria.Parameters.AddWithValue(
                                    "@registro",
                                    usuario);

                                comandoAuditoria.Parameters.AddWithValue(
                                    "@valorAnterior",
                                    valorAnterior);

                                comandoAuditoria.Parameters.AddWithValue(
                                    "@novoValor",
                                    novoValor);

                                comandoAuditoria.ExecuteNonQuery();
                            }

                            // CONFIRMA TRANSAÇÃO

                            transacao.Commit();
                        }
                        catch
                        {
                            transacao.Rollback();
                            throw;
                        }
                    }

                    // MENSAGEM

                    if (primeiroCadastro)
                    {
                        MessageBox.Show(
                            "Primeiro usuário cadastrado com sucesso!\n\n" +
                            "O usuário foi definido automaticamente como:\n" +
                            "Administrador\n" +
                            "Ativo\n\n" +
                            "O cadastro também foi registrado na auditoria.",
                            "Cadastro concluído",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Usuário cadastrado com sucesso!\n\n" +
                            "O cadastro também foi registrado na auditoria.",
                            "Cadastro concluído",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }

                // NAVEGAÇÃO APÓS CADASTRO

                // PRIMEIRO USUÁRIO
                // Vai para a tela de login para fazer o primeiro acesso.
                if (primeiroCadastro)
                {
                    MainWindow login =
                        new MainWindow();

                    login.Show();

                    Close();

                    return;
                }
                // ADMINISTRADOR CADASTROU UM NOVO USUÁRIO

                if (administradorCadastrando &&
                    Sessao.EhAdministrador)
                {
                    TelaPrincipal tela =
                        new TelaPrincipal();

                    tela.Show();

                    Close();

                    return;
                }

                MainWindow loginFinal =
                    new MainWindow();

                loginFinal.Show();

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
            // Se o cadastro foi aberto pelo administrador,
            // volta para a TelaPrincipal.
            if (administradorCadastrando &&
                Sessao.EhAdministrador)
            {
                TelaPrincipal tela =
                    new TelaPrincipal();

                tela.Show();

                Close();

                return;
            }

            // Primeiro cadastro ou cadastro sem administrador
            // volta para a tela de login.
            MainWindow login =
                new MainWindow();

            login.Show();

            Close();
        }
    }
}