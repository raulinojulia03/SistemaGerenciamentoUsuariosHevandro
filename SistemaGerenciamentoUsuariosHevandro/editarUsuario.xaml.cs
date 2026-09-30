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

        public EditarUsuario(
            int id,
            bool administrador)
        {
            InitializeComponent();

            idUsuario = id;
            administradorEditando = administrador;

            CarregarUsuario();
        }

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

                if (administradorEditando)
                {
                    // Administrador pode alterar
                    // tipo e status
                    cmbTipo.IsEnabled = true;
                    cmbStatus.IsEnabled = true;
                    txtUsuario.IsEnabled = true;
                }
                else
                {
                    // Usuário comum só pode alterar
                    // seus próprios dados básicos
                    cmbTipo.IsEnabled = false;
                    cmbStatus.IsEnabled = false;
                    txtUsuario.IsEnabled = false;
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

        private void SelecionarCombo(
            ComboBox combo,
            string valor)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                // Avatar usa Tag
                if (combo == cmbAvatar)
                {
                    if (item.Tag != null &&
                        item.Tag.ToString() == valor)
                    {
                        combo.SelectedItem = item;
                        break;
                    }
                }
                // Tipo e Status usam Content
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

            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show(
                    "O nome é obrigatório.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MessageBox.Show(
                    "O usuário é obrigatório.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(email) ||
                !email.Contains("@"))
            {
                MessageBox.Show(
                    "Informe um e-mail válido.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(tipo))
            {
                MessageBox.Show(
                    "Selecione o tipo de usuário.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                MessageBox.Show(
                    "Selecione o status.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

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

                    // VERIFICA USUÁRIO OU E-MAIL DUPLICADO

                    string verificar = @"
                        SELECT COUNT(*)
                        FROM usuarios
                        WHERE (usuario = @usuario
                        OR email = @email)
                        AND id <> @id";

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

                        comandoVerificar.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        int quantidade =
                            Convert.ToInt32(
                                comandoVerificar.ExecuteScalar());

                        if (quantidade > 0)
                        {
                            MessageBox.Show(
                                "O usuário ou e-mail já está cadastrado para outro usuário.",
                                "Cadastro existente",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }
                    }

                    // ALTERA USUÁRIO

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

                    // AUDITORIA

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
                            NULL,
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
                            "@novo",
                            "Tipo: " + tipo +
                            " | Status: " + status);

                        comando.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Usuário alterado com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                TelaUsuarios tela =
                    new TelaUsuarios();

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

        private void Cancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            TelaUsuarios tela =
                new TelaUsuarios();

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