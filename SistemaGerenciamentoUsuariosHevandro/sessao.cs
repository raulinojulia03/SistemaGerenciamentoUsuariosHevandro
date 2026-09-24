namespace WpfApp1
{
    public static class Sessao
    {
        public static int Id { get; set; }

        public static string Usuario { get; set; } = "";

        public static string NomeCompleto { get; set; } = "";

        public static string Email { get; set; } = "";

        public static string TipoUsuario { get; set; } = "";

        public static string Perfil { get; set; } = "";

        public static string Status { get; set; } = "";

        public static string Avatar { get; set; } = "";

        public static bool EhAdministrador { get; set; }

        // Compatibilidade com códigos que usam IsADM
        public static bool IsADM
        {
            get => EhAdministrador;
            set => EhAdministrador = value;
        }

        // Compatibilidade caso algum código use IdUsuario
        public static int IdUsuario
        {
            get => Id;
            set => Id = value;
        }

        public static void Limpar()
        {
            Id = 0;
            Usuario = "";
            NomeCompleto = "";
            Email = "";
            TipoUsuario = "";
            Perfil = "";
            Status = "";
            Avatar = "";
            EhAdministrador = false;
        }
    }
}