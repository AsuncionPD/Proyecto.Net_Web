using AppWebBeachSA.Data;
using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace AppWebBeachSA.Controllers
{
    public class EmpleadosController : Controller
    {
        //Variable para utilizar la API
        private HotelAPI hotelAPI;

        //Variable para manejar las transacciones del protocolo HttpClient
        private HttpClient httpClient;

        public EmpleadosController()
        {
            hotelAPI = new HotelAPI();
            httpClient = hotelAPI.Inicial();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind] Empleado empleado)
        {
            AutorizacionResponse autorizacion = null;

            if (empleado == null)
            {
                TempData["Error"] = "Usuario o contraseña incorrecta";
                return View(empleado);
            }


            HttpResponseMessage response = await httpClient.PostAsync($"/Empleados/Login?email={empleado.Email}&password={empleado.Password}", null);

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                autorizacion = JsonConvert.DeserializeObject<AutorizacionResponse>(resultado);
            }

            if (autorizacion != null && autorizacion.Resultado == true)
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, empleado.Email));
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                HttpContext.Session.SetString("token", autorizacion.Token);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Error"] = "Usuario o contraseña incorrecta";
                return View(empleado);
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
        public async Task<IActionResult> CrearCuenta([Bind] Empleado empleado, string confirmPassword)
        {
            var response = await httpClient.PostAsJsonAsync($"Empleados/CrearCuenta?confirmPassword={confirmPassword}", empleado);

            var resultContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = resultContent;
                return View(empleado);
            }

            return RedirectToAction("Login", "Empleados");
        }
    }
}
