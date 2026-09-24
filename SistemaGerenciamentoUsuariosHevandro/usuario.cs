using System;

namespace WpfApp1
{
    public class Usuario
    {
        public int Id { get; set; }

        public string UsuarioNome { get; set; } = "";

        public string NomeCompleto { get; set; } = "";

        public string Email { get; set; } = "";

        public string TipoUsuario { get; set; } = "";

        public string Perfil { get; set; } = "";

        public string Status { get; set; } = "";

        public string Avatar { get; set; } = "";

        public string Senha { get; set; } = "";

        public bool IsADM { get; set; }

        public DateTime? DataCriacao { get; set; }

        public DateTime? DataAlteracao { get; set; }

        public DateTime? UltimoLogin { get; set; }

        public string UltimoLoginTexto
        {
            get
            {
                if (UltimoLogin.HasValue)
                    return UltimoLogin.Value.ToString("dd/MM/yyyy HH:mm");

                return "Nunca acessou";
            }
        }
    }
}