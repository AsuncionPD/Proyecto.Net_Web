using Microsoft.AspNetCore.Mvc;
using AppWebBeachSA.Models;
using AppWebBeachSA.Data;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace AppWebBeachSA.Controllers
{
    public class PaquetesController : Controller
    {
        //Variable para utilizar la API
        private HotelAPI hotelAPI;

        //Variable para manejar las transacciones del protocolo HttpClient
        private HttpClient httpClient;


        public PaquetesController()
        {
            hotelAPI = new HotelAPI();
            httpClient = hotelAPI.Inicial();
        }

        /// <summary>
        /// metodo para obtener el token de autenticacion 
        /// </summary>
        /// <returns></returns>
        private AuthenticationHeaderValue AutorizacionToken()
        {
            var token = HttpContext.Session.GetString("token");
            AuthenticationHeaderValue authentication = null;

            if (token != null && token.Length != 0)
            {
                authentication = new AuthenticationHeaderValue("Bearer", token);
            }

            return authentication;
        }

        /// <summary>
        /// metodo encargado de obtener la lista de paquetes 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            List<Paquete> paquetes = new List<Paquete>();

            HttpResponseMessage response = await httpClient.GetAsync("/Paquetes/Listado");

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                paquetes = JsonConvert.DeserializeObject<List<Paquete>>(resultado);
            }

            return View(paquetes);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Metodo encargado de agregar un paquete utilizando la API web
        /// </summary>
        /// <param name="paquete"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Paquete paquete)
        {
            paquete.PaqueteID = 0;

            //se asigna el token de autorización
            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            var agregar = httpClient.PostAsJsonAsync<Paquete>("/Paquetes/Agregar", paquete);
            await agregar; 

            var resultado = agregar.Result;
            if (resultado.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Mensaje"] = "No se logró registrar el paquete";
                return View(paquete);
            }

        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var paquete = new Paquete();

            HttpResponseMessage response = await httpClient.GetAsync($"/Paquetes/Buscar?id={id}");

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                paquete = JsonConvert.DeserializeObject<Paquete>(resultado);
            }

            return View(paquete);
        }

        /// <summary>
        /// Metodo encargado de editar un paquete
        /// </summary>
        /// <param name="paquete"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind] Paquete paquete)
        {
            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            var modificar = httpClient.PutAsJsonAsync<Paquete>("/Paquetes/Editar", paquete);
            await modificar;

            var resultado = modificar.Result;
            if (resultado.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Mensaje"] = "Datos incorrectos";

                if (resultado.StatusCode.ToString().Equals("Unauthorized"))
                {
                    return RedirectToAction("Login", "Empleados");
                }
                else
                {
                    return View(paquete);
                }
            }
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var paquete = new Paquete();

            HttpResponseMessage response = await httpClient.GetAsync($"/Paquetes/Buscar?id={id}");

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                paquete = JsonConvert.DeserializeObject<Paquete>(resultado);
            }

            return View(paquete);
        }


        /// <summary>
        /// Metodo encargado de eliminar un paquete por medio del id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePack(int id)
        {
            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await httpClient.DeleteAsync($"/Paquetes/Eliminar?id={id}");

            if (response.StatusCode.ToString().Equals("Unauthorized"))
            {
                return RedirectToAction("Login", "Empleados");
            }
            else
            { 
                return RedirectToAction("Index");
            }
        }


    }
}
