using MySql.Data.MySqlClient;

namespace WpfApp1
{
    public static class Banco
    {
        private static readonly string connectionString =
            "Server=localhost;Database=login;Uid=root;Pwd=;";

        public static MySqlConnection CriarConexao()
        {
            return new MySqlConnection(connectionString);
        }
    }
}