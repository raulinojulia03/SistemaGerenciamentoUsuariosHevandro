using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class EditarUsuario : Window
    {
        private int idUsuario;
        private bool administradorEditando;
        private bool editandoPropriaConta;

        public EditarUsuario(
            int id,
            bool administrador)
        {
            InitializeComponent();

            idUsuario = id;
            administradorEditando = administrador;

            editandoPropriaConta =
                idUsuario == Sessao.Id;

            CarregarUsuario();
        }

        // CARREGAR USUÁRIO

        private void CarregarUsuario()
        {
            try
            {
                using (MySqlConnection conexao =
                    Banco.CriarConexao())
                {
                    conexao.Open();

                    string query = @"
                        SELECT
                            nome_completo,
                            usuario,
                            email,
                            tipo_usuario,
                            status,
                            avatar
                        FROM usuarios
                        WHERE id = @id";

                    using (MySqlCommand comando =
                        new MySqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        using (MySqlDataReader reader =
                            comando.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show(
                                    "Usuário não encontrado.",
                                    "Atenção",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                Close();
                                return;
                            }

                            txtNome.Text =
                                reader["nome_completo"].ToString();

                            txtUsuario.Text =
                                reader["usuario"].ToString();

                            txtEmail.Text =
                                reader["email"].ToString();

                            SelecionarCombo(
                                cmbTipo,
                                reader["tipo_usuario"].ToString());

                            SelecionarCombo(
                                cmbStatus,
                                reader["status"].ToString());

                            SelecionarCombo(
                                cmbAvatar,
                                reader["avatar"].ToString());
                        }
                    }
                }

                // PERMISSÕES

                if (editandoPropriaConta)
                {
                    cmbTipo.IsEnabled = false;
                    cmbStatus.IsEnabled = false;
                    txtUsuario.IsEnabled = false;

                    btnAlterarSenha.Visibility =
                        Visibility.Visible;
                }
                else if (administradorEditando)
                {
                    cmbTipo.IsEnabled = true;
                    cmbStatus.IsEnabled = true;
                    txtUsuario.IsEnabled = true;

                    btnAlterarSenha.Visibility =
                        Visibility.Visible;
                }
                else
                {
                    cmbTipo.IsEnabled = false;
                    cmbStatus.IsEnabled = false;
                    txtUsuario.IsEnabled = false;

                    btnAlterarSenha.Visibility =
                        Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao carregar usuário:\n\n" +
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // SELECIONAR COMBO

        private void SelecionarCombo(
            ComboBox combo,
            string valor)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (combo == cmbAvatar)
                {
                    if (item.Tag != null &&
                        item.Tag.ToString() == valor)
                    {
                        combo.SelectedItem = item;
                        break;
                    }
                }
                else
                {
                    if (item.Content != null &&
                        item.Content.ToString() == valor)
                    {
                        combo.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        // SALVAR

        private void Salvar_Click(
            object sender,
            RoutedEventArgs e)
        {
            string nome =
                txtNome.Text.Trim();

            string usuario =
                txtUsuario.Text.Trim();

            string email =
                txtEmail.Text.Trim();

            string tipo =
                ((ComboBoxItem)cmbTipo.SelectedItem)
                ?.Content.ToString();

            string status =
                ((ComboBoxItem)cmbStatus.SelectedItem)
                ?.Content.ToString();

            string avatar =
                ((ComboBoxItem)cmbAvatar.SelectedItem)
                ?.Tag?.ToString();

            // VALIDAÇÃO DO NOME

            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show(
                    "O nome completo é obrigatório.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtNome.Focus();
                return;
            }

            // VALIDAÇÃO DO USUÁRIO

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MessageBox.Show(
                    "O usuário é obrigatório.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtUsuario.Focus();
                return;
            }

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

            // VALIDAÇÃO DO E-MAIL

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "O e-mail é obrigatório.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtEmail.Focus();
                return;
            }

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

            // VALIDAÇÃO DO TIPO

            if (string.IsNullOrWhiteSpace(tipo))
            {
                MessageBox.Show(
                    "Selecione o tipo de usuário.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // VALIDAÇÃO DO STATUS

            if (string.IsNullOrWhiteSpace(status))
            {
                MessageBox.Show(
                    "Selecione o status.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // VALIDAÇÃO DO AVATAR

            if (string.IsNullOrWhiteSpace(avatar))
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

                    // BUSCA OS VALORES ANTERIORES

                    string nomeAnterior = "";
                    string usuarioAnterior = "";
                    string emailAnterior = "";
                    string tipoAnterior = "";
                    string statusAnterior = "";
                    string avatarAnterior = "";

                    string consultaAnterior = @"
                        SELECT
                            nome_completo,
                            usuario,
                            email,
                            tipo_usuario,
                            status,
                            avatar
                        FROM usuarios
                        WHERE id = @id";

                    using (MySqlCommand comandoAnterior =
                        new MySqlCommand(
                            consultaAnterior,
                            conexao))
                    {
                        comandoAnterior.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        using (MySqlDataReader reader =
                            comandoAnterior.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show(
                                    "Usuário não encontrado.",
                                    "Atenção",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            nomeAnterior =
                                reader["nome_completo"].ToString();

                            usuarioAnterior =
                                reader["usuario"].ToString();

                            emailAnterior =
                                reader["email"].ToString();

                            tipoAnterior =
                                reader["tipo_usuario"].ToString();

                            statusAnterior =
                                reader["status"].ToString();

                            avatarAnterior =
                                reader["avatar"].ToString();
                        }
                    }

                    // PROTEÇÃO DO PRÓPRIO ADMINISTRADOR

                    if (editandoPropriaConta &&
                        Sessao.EhAdministrador)
                    {
                        tipo =
                            tipoAnterior;

                        status =
                            statusAnterior;
                    }

                    // VERIFICA USUÁRIO OU E-MAIL DUPLICADO

                    string verificar = @"
                        SELECT
                            usuario,
                            email
                        FROM usuarios
                        WHERE id <> @id
                        AND (
                            usuario = @usuario
                            OR email = @email
                        )
                        LIMIT 1";

                    using (MySqlCommand comandoVerificar =
                        new MySqlCommand(
                            verificar,
                            conexao))
                    {
                        comandoVerificar.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        comandoVerificar.Parameters.AddWithValue(
                            "@usuario",
                            usuario);

                        comandoVerificar.Parameters.AddWithValue(
                            "@email",
                            email);

                        using (MySqlDataReader reader =
                            comandoVerificar.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string usuarioExistente =
                                    reader["usuario"].ToString();

                                string emailExistente =
                                    reader["email"].ToString();

                                if (usuarioExistente == usuario)
                                {
                                    reader.Close();

                                    MessageBox.Show(
                                        "Este nome de usuário já está cadastrado.",
                                        "Usuário existente",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);

                                    txtUsuario.Focus();
                                    return;
                                }

                                if (emailExistente == email)
                                {
                                    reader.Close();

                                    MessageBox.Show(
                                        "Este e-mail já está cadastrado.",
                                        "E-mail existente",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);

                                    txtEmail.Focus();
                                    return;
                                }
                            }
                        }
                    }

                    // IDENTIFICA SOMENTE O QUE FOI ALTERADO

                    string valorAnterior = "";
                    string novoValor = "";

                    // NOME
                    if (nomeAnterior != nome)
                    {
                        valorAnterior +=
                            "Nome: " +
                            nomeAnterior +
                            "\n";

                        novoValor +=
                            "Nome: " +
                            nome +
                            "\n";
                    }

                    // USUÁRIO
                    if (usuarioAnterior != usuario)
                    {
                        valorAnterior +=
                            "Usuário: " +
                            usuarioAnterior +
                            "\n";

                        novoValor +=
                            "Usuário: " +
                            usuario +
                            "\n";
                    }

                    // E-MAIL
                    if (emailAnterior != email)
                    {
                        valorAnterior +=
                            "E-mail: " +
                            emailAnterior +
                            "\n";

                        novoValor +=
                            "E-mail: " +
                            email +
                            "\n";
                    }

                    // TIPO
                    if (tipoAnterior != tipo)
                    {
                        valorAnterior +=
                            "Tipo: " +
                            tipoAnterior +
                            "\n";

                        novoValor +=
                            "Tipo: " +
                            tipo +
                            "\n";
                    }

                    // STATUS
                    if (statusAnterior != status)
                    {
                        valorAnterior +=
                            "Status: " +
                            statusAnterior +
                            "\n";

                        novoValor +=
                            "Status: " +
                            status +
                            "\n";
                    }

                    // AVATAR
                    if (avatarAnterior != avatar)
                    {
                        valorAnterior +=
                            "Avatar: " +
                            avatarAnterior +
                            "\n";

                        novoValor +=
                            "Avatar: " +
                            avatar +
                            "\n";
                    }

                    valorAnterior =
                        valorAnterior.TrimEnd(
                            '\r',
                            '\n');

                    novoValor =
                        novoValor.TrimEnd(
                            '\r',
                            '\n');

                    // ATUALIZA USUÁRIO

                    string query = @"
                        UPDATE usuarios
                        SET
                            nome_completo = @nome,
                            usuario = @usuario,
                            email = @email,
                            tipo_usuario = @tipo,
                            status = @status,
                            avatar = @avatar,
                            data_alteracao = NOW()
                        WHERE id = @id";

                    using (MySqlCommand comando =
                        new MySqlCommand(
                            query,
                            conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@nome",
                            nome);

                        comando.Parameters.AddWithValue(
                            "@usuario",
                            usuario);

                        comando.Parameters.AddWithValue(
                            "@email",
                            email);

                        comando.Parameters.AddWithValue(
                            "@tipo",
                            tipo);

                        comando.Parameters.AddWithValue(
                            "@status",
                            status);

                        comando.Parameters.AddWithValue(
                            "@avatar",
                            avatar);

                        comando.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        comando.ExecuteNonQuery();
                    }

                    // ATUALIZA SESSÃO

                    if (editandoPropriaConta)
                    {
                        Sessao.NomeCompleto =
                            nome;

                        Sessao.Email =
                            email;

                        Sessao.Avatar =
                            avatar;

                        Sessao.Status =
                            status;

                        Sessao.TipoUsuario =
                            tipo;
                    }

                    // AUDITORIA
                    // SÓ GRAVA SE ALGUM CAMPO MUDOU

                    if (!string.IsNullOrWhiteSpace(valorAnterior))
                    {
                        string auditoria = @"
                            INSERT INTO auditoria
                            (
                                usuario_responsavel,
                                operacao,
                                registro_afetado,
                                valor_anterior,
                                novo_valor
                            )
                            VALUES
                            (
                                @responsavel,
                                'ALTERACAO',
                                @afetado,
                                @anterior,
                                @novo
                            )";

                        using (MySqlCommand comando =
                            new MySqlCommand(
                                auditoria,
                                conexao))
                        {
                            comando.Parameters.AddWithValue(
                                "@responsavel",
                                Sessao.Usuario);

                            comando.Parameters.AddWithValue(
                                "@afetado",
                                usuario);

                            comando.Parameters.AddWithValue(
                                "@anterior",
                                valorAnterior);

                            comando.Parameters.AddWithValue(
                                "@novo",
                                novoValor);

                            comando.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show(
                    "Usuário alterado com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                TelaPrincipal tela =
                    new TelaPrincipal();

                tela.Show();

                Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Erro ao alterar usuário:\n\n" +
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

        private void AlterarSenha_Click(
            object sender,
            RoutedEventArgs e)
        {
            RedefinirSenha tela =
                new RedefinirSenha(idUsuario);

            tela.Show();

            Close();
        }

        private void Cancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            TelaPrincipal tela =
                new TelaPrincipal();

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