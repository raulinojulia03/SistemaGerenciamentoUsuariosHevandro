using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;

namespace WpfApp1
{
    public partial class Auditoria : Window
    {
        public Auditoria()
        {
            InitializeComponent();

            CarregarAuditoria();
        }

        private void CarregarAuditoria()
        {
            try
            {
                using (MySqlConnection conexao =
                    Banco.CriarConexao())
                {
                    conexao.Open();

                    string query = @"
                        SELECT
                            id,
                            data_hora,
                            usuario_responsavel,
                            operacao,
                            registro_afetado,
                            valor_anterior,
                            novo_valor
                        FROM auditoria
                        ORDER BY data_hora DESC";

                    using (MySqlDataAdapter adapter =
                        new MySqlDataAdapter(
                            query,
                            conexao))
                    {
                        DataTable tabela =
                            new DataTable();

                        adapter.Fill(tabela);

                        gridAuditoria.ItemsSource =
                            tabela.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao carregar auditoria:\n" +
                    ex.Message);
            }
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