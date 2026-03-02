namespace PrimerExamen.Models
{
    public class ApiUser
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public Direccion Direccion { get; set; }
        public string Telefono { get; set; }
        public Empresa Empresa { get; set; }
    }

    public class Direccion
    {
        public string Ciudad { get; set; }
        public Geo Geo { get; set; }
    }

    public class Empresa
    {
        public string Nombre { get; set; }
    }

    public class Geo
    {
        public string Lat { get; set; }
        public string Lng { get; set; }
    }
}
