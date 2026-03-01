using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.Configuration.UserSecrets;
using PrimerExamen.Models;
using System.Text.Json;

namespace PrimerExamen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public UsersController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        //GET: api/usuarios
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetContactos()
        {
            try
            {
                var response = await _httpClient.GetAsync("/users");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error al obtener los usuarios externos.");
                }

                string jsonString = await response.Content.ReadAsStringAsync();

                var usuariosExternos = JsonSerializer.Deserialize<List<JsonPlaceholderUser>>(jsonString, _jsonOptions);

                if (usuariosExternos == null || usuariosExternos.Count == 0)
                {
                    return NotFound("No se encontraron usuarios.");
                }

                var usuariosResponse = usuariosExternos.Select(u => new User
                {
                    Id = u.Id,
                    NombreCompleto = u.Name, 
                    Username = u.Username,
                    Email = u.Email,
                    Ciudad = u.Address?.City ?? "Desconocida", 
                    Empresa = u.Company?.Name ?? "Desconocida", 
                    Telefono = u.Phone != null ? u.Phone.Replace("x", "ext.") : "" 
                }).ToList();

                usuariosResponse = usuariosResponse.OrderBy(u => u.NombreCompleto).ToList();

                var finalResponse = new
                {
                    TotalUsuarios = usuariosResponse.Count,
                    DistribucionPorCiudad = usuariosResponse.Count > 5 ?
                        usuariosResponse.GroupBy(u => u.Ciudad)
                        .ToDictionary(g => g.Key, g => g.Count()) : null,
                    Usuarios = usuariosResponse
                };

                return Ok(finalResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        public class JsonPlaceholderUser
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public AddressData Address { get; set; }
            public CompanyData Company { get; set; }
        }

        public class AddressData
        {
            public string City { get; set; }
        }

        public class CompanyData
        {
            public string Name { get; set; }
        }
    }
}
