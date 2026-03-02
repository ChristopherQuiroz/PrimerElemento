using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.Configuration.UserSecrets;
using PrimerExamen.Models;
using System.Runtime.CompilerServices;
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

        #region usuario
        //GET: api/usuarios
        [HttpGet]
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

                var usuariosExternos = JsonSerializer.Deserialize<List<ApiUser>>(jsonString, _jsonOptions);

                if (usuariosExternos == null || usuariosExternos.Count == 0)
                {
                    return NotFound("No se encontraron usuarios.");
                }

                var usuariosResponse = usuariosExternos.Select(u => new User
                {
                    Id = u.Id,
                    NombreCompleto = u.Nombre,
                    Username = u.Username,
                    Email = u.Email,
                    Ciudad = u.Direccion?.Ciudad ?? "Desconocida",
                    Empresa = u.Empresa?.Nombre ?? "Desconocida",
                    Telefono = FormatearTelefono(u.Telefono)
                }).ToList();

                usuariosResponse = usuariosResponse.OrderBy(u => u.NombreCompleto).ToList();

                var Response = new
                {
                    TotalUsuarios = usuariosResponse.Count,
                    DistribucionPorCiudad = usuariosResponse.Count > 5 ?
                        usuariosResponse.GroupBy(u => u.Ciudad)
                        .ToDictionary(g => g.Key, g => g.Count()) : null,
                    Usuarios = usuariosResponse
                };

                return Ok(JsonHelper.ToJson(Response));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"No se pudo conectar con el servicio: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
        #endregion

        #region formatear telefono
        private string FormatearTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return "Desconocido";
            }
            else
            {
                return telefono.Replace("x", "ext. ");
            }
        }
        #endregion

        #region usuarios por ciudad
        //GET /api/usuarios/ciudad?nombre={texto}
        [HttpGet("/ciudad")]
        public async Task<IActionResult> GetUsuariosPorCiudad([FromQuery] string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return BadRequest(new
                    {
                        message = "El parámetro 'nombre' es requerido.",
                        example = "/api/usuarios/ciudad?nombre=Gwenborough"
                    });
                }

                var response = await _httpClient.GetAsync("/users");
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(503, new
                    {
                        message = "Error al obtener los usuarios externos."
                    });
                }

                string jsonString = await response.Content.ReadAsStringAsync();
                var usuariosExternos = JsonSerializer.Deserialize<List<ApiUser>>(jsonString, _jsonOptions);

                if (usuariosExternos == null || usuariosExternos.Count == 0)
                {
                    return NotFound(new
                    {
                        message = "No se encontraron usuarios."
                    });
                }

                var usuariosFiltrados = usuariosExternos.Where(u => u.Direccion.Ciudad != null && u.Direccion.Ciudad.Contains(nombre, StringComparison.OrdinalIgnoreCase)).ToList();

                var ciudades = usuariosFiltrados.Select(u => new
                {
                    Id = u.Id,
                    NombreCompleto = u.Nombre,
                    Ciudad = u.Direccion.Ciudad,
                    Coordenadas = $"{u.Direccion.Geo.Lat ?? "0"}, {u.Direccion.Geo.Lng ?? "0"}"
                }).ToList();

                var responseObj = new
                {
                    TotalUsuarios = ciudades.Count,
                    CiudadBuscada = nombre,
                    Usuarios = ciudades
                };

                return Ok(JsonHelper.ToJson(responseObj));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new
                {
                    message = $"No se pudo conectar con el servicio: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }
        #endregion

        #region usuarios cercanos
        //GET /api/usuarios/cercanos?lat={lat}&lng={lng}&radio={km}
        [HttpGet("/cercanos")]
        public async Task<IActionResult> GetUsuariosCercanos(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radio)
        {
            try
            {
                if (lat < -90 || lat > 90 || lng < -180 || lng > 180 || radio <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Parámetros inválidos. Latitud debe estar entre -90 y 90, longitud entre -180 y 180, y radio debe ser mayor a 0.",
                        example = "/api/usuarios/cercanos?lat=40.7128&lng=-74.0060&radio=50"
                    });
                }

                if (radio <= 0)
                {
                    return BadRequest(new
                    {
                        message = "El parámetro 'radio' debe ser mayor a 0.",
                        example = "/api/usuarios/cercanos?lat=40.7128&lng=-74.0060&radio=50"
                    });
                }

                var response = await _httpClient.GetAsync("/users");
                if(!response.IsSuccessStatusCode)
                {
                    return StatusCode(503, new
                    {
                        message = "Error al obtener los usuarios externos."
                    });
                }

                string jsonString = await response.Content.ReadAsStringAsync();

                var usuariosExternos = JsonSerializer.Deserialize<List<ApiUser>>(jsonString, _jsonOptions);
                if(usuariosExternos == null || usuariosExternos.Count == 0)
                {
                    return NotFound(new
                    {
                        message = "No se encontraron usuarios."
                    });
                }

                var usuariosCercanos = new List<dynamic>();

                foreach (var usuario in usuariosExternos)
                {
                    if(!double.TryParse(usuario.Direccion.Geo.Lat, out double usuarioLat) || !double.TryParse(usuario.Direccion.Geo.Lng, out double usuarioLng))
                    {
                        continue;
                    }

                    double distancia = Math.Sqrt(Math.Pow(usuarioLat - lat, 2) + Math.Pow(usuarioLng - lng, 2)) * 100;

                    if(distancia <= radio)
                    {
                        usuariosCercanos.Add(new
                        {
                            Id = usuario.Id,
                            NombreCompleto = usuario.Nombre,
                            Ciudad = usuario.Direccion.Ciudad,
                            Coordenadas = $"{usuarioLat}, {usuarioLng}",
                            DistanciaKm = Math.Round(distancia, 2),
                            UbicacionLink = $"https://maps.google.com/?q-{usuario.Direccion.Geo.Lat},{usuario.Direccion.Geo.Lng}"
                        });
                    }
                }

                var usuariosOrdenados = usuariosCercanos
                    .OrderBy(u => u.DistanciaKm)
                    .Select(u=> new
                    {
                        u.Id,
                        u.NombreCompleto,
                        u.Ciudad,
                        u.Coordenadas,
                        u.DistanciaKm,
                        u.UbicacionLink
                    })
                    .ToList();

                var responseObj = new
                {
                    CentroBusqueda = new { lat, lng },
                    Radio = radio,
                    TotalEncontrados = usuariosOrdenados.Count,
                    Usuarios = usuariosOrdenados
                };

                return Ok(JsonHelper.ToJson(responseObj));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new
                {
                    message = $"No se pudo conectar con el servicio: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }
        #endregion


    }
}
