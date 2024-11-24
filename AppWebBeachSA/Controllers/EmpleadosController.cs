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

        public async Task<IActionResult> Home()
        {
            int packageCount = 0;
            int employeeCount = 0;
            int reservationCount = 0;

            try
            {
                HttpResponseMessage packageResponse = await httpClient.GetAsync("/Paquetes/Count");
                if (packageResponse.IsSuccessStatusCode)
                {
                    var result = await packageResponse.Content.ReadAsStringAsync();
                    packageCount = int.Parse(result); 
                }

                HttpResponseMessage employeeResponse = await httpClient.GetAsync("/Empleados/Count");
                if (employeeResponse.IsSuccessStatusCode)
                {
                    var result = await employeeResponse.Content.ReadAsStringAsync();
                    employeeCount = int.Parse(result);
                }

                HttpResponseMessage reservationResponse = await httpClient.GetAsync("/Reservaciones/Count");
                if (reservationResponse.IsSuccessStatusCode)
                {
                    var result = await reservationResponse.Content.ReadAsStringAsync();
                    reservationCount = int.Parse(result);  
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al obtener los datos: {ex.Message}";
            }

            ViewBag.PackageCount = packageCount;
            ViewBag.EmployeeCount = employeeCount;
            ViewBag.ReservationCount = reservationCount;

            return View();
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
                var tipoUsuario = autorizacion.TipoUsuario;
                HttpContext.Session.SetString("TipoUsuario", tipoUsuario.ToString());

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, empleado.Email));
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                HttpContext.Session.SetString("token", autorizacion.Token);

                return RedirectToAction("Home", "Empleados");
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
