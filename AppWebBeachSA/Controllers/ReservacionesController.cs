using AppWebBeachSA.Data;
using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace AppWebBeachSA.Controllers
{
    public class ReservacionesController : Controller
    {
        private HotelAPI hotelAPI;

        //Variable para manejar las transacciones del protocolo HttpClient
        private HttpClient httpClient;

        public ReservacionesController()
        {
            hotelAPI = new HotelAPI();
            httpClient = hotelAPI.Inicial();
        }


        public async Task<IActionResult> Index()
        {
            List<Reservacion> reservaciones = new List<Reservacion>();
            int userId = int.Parse(User.FindFirstValue("UserId"));
            HttpResponseMessage response = await httpClient.GetAsync($"/Reservaciones/ReservacionesCliente?id={userId}");

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                reservaciones = JsonConvert.DeserializeObject<List<Reservacion>>(resultado);
            }

            if (reservaciones != null) {
                return View(reservaciones);
            }

            return RedirectToAction("reservar");
        }

        [HttpGet]

        public async Task<IActionResult> reservar()
        {
            HttpResponseMessage response = await httpClient.GetAsync("/Paquetes/Listado");
            List<Paquete> paquetes = new List<Paquete>();
            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                  paquetes = JsonConvert.DeserializeObject<List<Paquete>>(resultado);
                ViewBag.Paquetes = paquetes;

            }
            var tipoCambio = await httpClient.GetAsync("https://apis.gometa.org/tdc/tdc.json");
            if (tipoCambio.IsSuccessStatusCode)
            {
                var content = await tipoCambio.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(content);
                ViewBag.tipoCambio= data.venta;
            }

                return View();

        }




    }
}
