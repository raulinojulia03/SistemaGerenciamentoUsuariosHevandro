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

        // Guarda o último login anterior ao login atual
        public static string UltimoLogin { get; set; } = "";

        public static bool EhAdministrador { get; set; }

        // Compatibilidade com códigos antigos
        public static bool IsADM
        {
            get
            {
                return EhAdministrador;
            }
            set
            {
                EhAdministrador = value;
            }
        }

        // Compatibilidade com códigos que usam IdUsuario
        public static int IdUsuario
        {
            get
            {
                return Id;
            }
            set
            {
                Id = value;
            }
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
            UltimoLogin = "";
            EhAdministrador = false;
        }
    }
}