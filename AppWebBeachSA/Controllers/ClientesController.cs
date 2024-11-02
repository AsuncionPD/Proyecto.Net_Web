using Microsoft.AspNetCore.Mvc;
using AppWebBeachSA.Models;
using AppWebBeachSA.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System.Security.Claims;
using Newtonsoft.Json.Linq;

namespace AppWebBeachSA.Controllers
{
    public class ClientesController : Controller
    {
        //Variable para utilizar la API
        private HotelAPI hotelAPI;

        //Variable para manejar las transacciones del protocolo HttpClient
        private HttpClient httpClient;


        public ClientesController()
        {
            hotelAPI = new HotelAPI();
            httpClient = hotelAPI.Inicial();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind] Cliente cliente)
        {
            AutorizacionResponse autorizacion = null;

            if (cliente == null)
            {
                TempData["Error"] = "Usuario o contraseña incorrecta";
                return View(cliente);
            }


            HttpResponseMessage response = await httpClient.PostAsync($"/Clientes/Login?email={cliente.Email}&password={cliente.Password}", null);

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                autorizacion = JsonConvert.DeserializeObject<AutorizacionResponse>(resultado);
            }

            if (autorizacion != null && autorizacion.Resultado == true)
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, cliente.Email));
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                HttpContext.Session.SetString("token", autorizacion.Token);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Error"] = "Usuario o contraseña incorrecta";
                return View(cliente);
            }
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult CrearCuenta()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCuenta([Bind] Cliente cliente, string confirmPassword)
        {
            var response = await httpClient.PostAsJsonAsync($"Clientes/CrearCuenta?confirmPassword={confirmPassword}", cliente);

            var resultContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = resultContent;
                return View(cliente);
            }

            return RedirectToAction("Login", "Clientes");
        }


        [HttpGet]
        public IActionResult ObtenerDatosCliente()
        {
            return View();
        }

        public async Task<IActionResult> ObtenerDatosCliente(string cedula)
        {
            Cliente datosCliente = new Cliente();

            var response = await httpClient.GetAsync("https://apis.gometa.org/cedulas/" + cedula);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(content);

                if (data != null && data.results.Count > 0)
                {
                    var cliente = new Cliente
                    {
                        Nombre = data.nombre,
                        TipoCedula = data.results[0].guess_type,
                        Cedula = data.cedula,
                        Telefono = 0,
                        Direccion = "",
                        Email = "",
                        Password = "",
                    };
                    datosCliente = cliente;

                    TempData["Nombre"] = datosCliente.Nombre;
                    TempData["Cedula"] = datosCliente.Cedula;
                    TempData["TipoCedula"] = datosCliente.TipoCedula;

                    return RedirectToAction("CrearCuenta", "Clientes");
                }
                else
                {
                    TempData["Error"] = "No se encontraron datos para el número de cédula proporcionado.";
                }
            }
            else
            {
                TempData["Error"] = "Error al conectar con el servicio de datos.";
            }

            return View();
        }
    }
}
