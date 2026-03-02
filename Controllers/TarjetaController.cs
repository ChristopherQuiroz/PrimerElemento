using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PrimerExamen.Controllers
{
    [Route("api/usuarios")]
    [ApiController]
    public class TarjetaController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public TarjetaController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
        }

        [HttpGet("{id}/tarjeta")]
        public async Task<IActionResult> GetTarjeta(int id)
        {
            var response = await _httpClient.GetAsync($"/users/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            var content = await response.Content.ReadAsStringAsync();
            var u = System.Text.Json.JsonSerializer.Deserialize<UserExternal>(content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });


            string telFormateado = u.Phone.Replace("x", "ext.");

            string webFinal = u.Website.StartsWith("http") ? u.Website : $"https://{u.Website}";

            var resultado = new
            {
                tarjeta = new
                {
                    encabezado = new
                    {
                        nombre = u.Name.ToUpper(), 
                        usuario = $"@{u.Username}" 
                    },
                    contacto = new
                    {
                        email = u.Email,
                        telefono = telFormateado,
                        sitioWeb = webFinal
                    },
                    direccion = new
                    {
                        completa = $"{u.Address.Street}, {u.Address.Suite} - {u.Address.City}, {u.Address.Zipcode}",
                        geo = $"{u.Address.Geo.Lat}, {u.Address.Geo.Lng}"
                    },
                    empresa = new
                    {
                        nombre = u.Company.Name,
                        lema = $"\"{u.Company.CatchPhrase}\"",
                        giro = u.Company.Bs
                    },
                    mapa = $"https://maps.google.com/?q={u.Address.Geo.Lat},{u.Address.Geo.Lng}"
                }
            };

            return Ok(resultado);
        }
    }

    public class UserExternal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public AddressData Address { get; set; }
        public CompanyData Company { get; set; }
    }

    public class AddressData
    {
        public string Street { get; set; }
        public string Suite { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public GeoData Geo { get; set; }
    }

    public class GeoData
    {
        public string Lat { get; set; }
        public string Lng { get; set; }
    }

    public class CompanyData
    {
        public string Name { get; set; }
        public string CatchPhrase { get; set; }
        public string Bs { get; set; }
    }
}
