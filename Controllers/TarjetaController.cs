using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrimerExamen.Models;

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
            try
            {
                var response = await _httpClient.GetAsync($"/users/{id}");

                if (!response.IsSuccessStatusCode)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                var content = await response.Content.ReadAsStringAsync();
                var u = System.Text.Json.JsonSerializer.Deserialize<ApiUser>(content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                string telFormateado = u.Telefono.Replace("x", "ext. ");

                string webFinal = u.PaginaWeb.StartsWith("http") ? u.PaginaWeb : $"https://{u.PaginaWeb}";

                var resultado = new
                {
                    tarjeta = new
                    {
                        encabezado = new
                        {
                            nombre = u.Nombre.ToUpper(),
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
                            completa = $"{u.Direccion.Calle}, {u.Direccion.Suite} - {u.Direccion.Ciudad}, {u.Direccion.CodigoPostal}",
                            geo = $"{u.Direccion.Geo.Lat}, {u.Direccion.Geo.Lng}"
                        },
                        empresa = new
                        {
                            nombre = u.Empresa.Nombre,
                            lema = $"\"{u.Empresa.frase}\"",
                            giro = u.Empresa.Calle
                        },
                        mapa = $"https://maps.google.com/?q={u.Direccion.Geo.Lat},{u.Direccion.Geo.Lng}"
                    }
                };

                return Ok(resultado);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { mensaje = "Error al conectar con el servicio externo", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }

        }

    }
}
