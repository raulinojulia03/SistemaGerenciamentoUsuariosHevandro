using MySql.Data.MySqlClient;

namespace WpfApp1
{
    public static class Banco
    {
        private static readonly string conexao =
            "Server=localhost;Database=login;Uid=root;Pwd=;";

        public static MySqlConnection CriarConexao()
        {
            return new MySqlConnection(conexao);
        }

        public static MySqlConnection Conectar()
        {
            return new MySqlConnection(conexao);
        }
    }
}