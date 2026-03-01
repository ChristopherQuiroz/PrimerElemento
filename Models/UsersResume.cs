namespace PrimerExamen.Models
{
    public class UsersResume
    {
        public int UsuarioId { get; set; }
        public string? _UsuarioName;
        public string UsuarioName
        {
            get=>_UsuarioName ??= $"Usuario {UsuarioId}";
            set=> _UsuarioName = value;
        }

        public int TotalUsuarios { get; set; }
        public List<User> Usuarios { get; set; } = new();

    }
}
