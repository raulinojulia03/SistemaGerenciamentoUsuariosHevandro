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
            string senha =
                txtSenha.Password;

            string confirmar =
                txtConfirmar.Password;

            if (senha.Length < 8)
            {
                MessageBox.Show(
                    "A senha deve possuir no mínimo 8 caracteres.");

                return;
            }

            if (senha != confirmar)
            {
                MessageBox.Show(
                    "As senhas não coincidem.");

                return;
            }

            try
            {
                string hash =
                    BCrypt.Net.BCrypt.HashPassword(
                        senha);

                using (MySqlConnection conexao =
                    Banco.CriarConexao())
                {
                    conexao.Open();

                    string query = @"
                        UPDATE usuarios
                        SET senha = @senha
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

                        comando.ExecuteNonQuery();
                    }

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
                        new MySqlCommand(
                            auditoria,
                            conexao))
                    {
                        comando.Parameters.AddWithValue(
                            "@responsavel",
                            Sessao.Usuario);

                        comando.Parameters.AddWithValue(
                            "@afetado",
                            idUsuario);

                        comando.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Senha redefinida com sucesso!");

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao redefinir senha:\n" +
                    ex.Message);
            }
        }
    }
}