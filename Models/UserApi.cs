using System.Text.Json.Serialization; 

namespace PrimerExamen.Models
{
    public class ApiUser
    {
        public int Id { get; set; }

        [JsonPropertyName("name")] 
        public string Nombre { get; set; }

        public string Username { get; set; } 

        public string Email { get; set; } 

        [JsonPropertyName("address")] 
        public Direccion Direccion { get; set; }

        [JsonPropertyName("phone")] 
        public string Telefono { get; set; }

        [JsonPropertyName("company")] 
        public Empresa Empresa { get; set; }
    }

    public class Direccion
    {
        [JsonPropertyName("city")] 
        public string Ciudad { get; set; }

        [JsonPropertyName("geo")]
        public Geo Geo { get; set; }
    }

    public class Empresa
    {
        [JsonPropertyName("name")]
        public string Nombre { get; set; }
    }

    public class Geo
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; }

        [JsonPropertyName("lng")]
        public string Lng { get; set; }
    }
}