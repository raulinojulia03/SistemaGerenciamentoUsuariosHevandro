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
                                    "Usuário não encontrado.");

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
                                reader["tipo_usuario"]
                                .ToString());

                            SelecionarCombo(
                                cmbStatus,
                                reader["status"]
                                .ToString());

                            SelecionarCombo(
                                cmbAvatar,
                                reader["avatar"]
                                .ToString());
                        }
                    }
                }

                // Usuário comum só pode editar o próprio perfil
                if (!Sessao.EhAdministrador)
                {
                    cmbTipo.IsEnabled = false;
                    cmbStatus.IsEnabled = false;

                    txtUsuario.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao carregar usuário:\n" +
                    ex.Message);
            }
        }

        private void SelecionarCombo(
            ComboBox combo,
            string valor)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Content.ToString() == valor)
                {
                    combo.SelectedItem = item;
                    break;
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
                ?.Content.ToString();

            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show(
                    "O nome é obrigatório.");

                return;
            }

            if (string.IsNullOrWhiteSpace(email) ||
                !email.Contains("@"))
            {
                MessageBox.Show(
                    "Informe um e-mail válido.");

                return;
            }

            try
            {
                using (MySqlConnection conexao =
                    Banco.CriarConexao())
                {
                    conexao.Open();

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
                        new MySqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@nome", nome);

                        comando.Parameters.AddWithValue(
                            "@usuario", usuario);

                        comando.Parameters.AddWithValue(
                            "@email", email);

                        comando.Parameters.AddWithValue(
                            "@tipo", tipo);

                        comando.Parameters.AddWithValue(
                            "@status", status);

                        comando.Parameters.AddWithValue(
                            "@avatar", avatar);

                        comando.Parameters.AddWithValue(
                            "@id", idUsuario);

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
                            "Dados do usuário alterados");

                        comando.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Usuário alterado com sucesso!");

                TelaUsuarios tela =
                    new TelaUsuarios();

                tela.Show();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao alterar usuário:\n" +
                    ex.Message);
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
    }
}