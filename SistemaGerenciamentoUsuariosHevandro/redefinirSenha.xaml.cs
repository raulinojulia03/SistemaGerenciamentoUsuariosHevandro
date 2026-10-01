using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace WpfApp1
{
    public partial class RedefinirSenha : Window
    {
        private int idUsuario;

        public RedefinirSenha(int id)
        {
            InitializeComponent();

            idUsuario = id;
        }

        private void Salvar_Click(
            object sender,
            RoutedEventArgs e)
        {
            string senha = txtSenha.Password;
            string confirmar = txtConfirmar.Password;

            // VALIDAÇÃO DA SENHA

            if (string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show(
                    "Informe a nova senha.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (senha.Length < 8)
            {
                MessageBox.Show(
                    "A senha deve possuir no mínimo 8 caracteres.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (senha != confirmar)
            {
                MessageBox.Show(
                    "As senhas não coincidem.",
                    "Atenção",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                string hash =
                    BCrypt.Net.BCrypt.HashPassword(senha);

                using (MySqlConnection conexao =
                    Banco.CriarConexao())
                {
                    conexao.Open();

                    // ALTERAR SENHA

                    string query = @"
                        UPDATE usuarios
                        SET
                            senha = @senha,
                            data_alteracao = NOW()
                        WHERE id = @id";

                    using (MySqlCommand comando =
                        new MySqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@senha",
                            hash);

                        comando.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        int linhasAfetadas =
                            comando.ExecuteNonQuery();

                        if (linhasAfetadas == 0)
                        {
                            MessageBox.Show(
                                "Usuário não encontrado.",
                                "Atenção",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }
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
                            'REDEFINICAO_SENHA',
                            @afetado,
                            NULL,
                            'Senha redefinida'
                        )";

                    using (MySqlCommand comando =
                        new MySqlCommand(auditoria, conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@responsavel",
                            Sessao.Usuario);

                        comando.Parameters.AddWithValue(
                            "@afetado",
                            idUsuario.ToString());

                        comando.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Senha redefinida com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                TelaPrincipal tela =
                    new TelaPrincipal();

                tela.Show();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao redefinir senha:\n\n" +
                    ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}