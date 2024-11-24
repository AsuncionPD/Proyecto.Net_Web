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
                 response = await httpClient.GetAsync("/Paquetes/Listado");
                List<Paquete> paquetes = new List<Paquete>();
                if (response.IsSuccessStatusCode)
                {
                    var resultado = response.Content.ReadAsStringAsync().Result;
                    paquetes = JsonConvert.DeserializeObject<List<Paquete>>(resultado);
                    ViewBag.Paquetes = paquetes;

                }
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

        [HttpGet]

        public async Task<IActionResult> paquete(int id)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"/Paquetes/Buscar?id={id}");
            Paquete paquete = new Paquete();
            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                paquete = JsonConvert.DeserializeObject<Paquete>(resultado);
                ViewBag.paquete= paquete;
            }
            var image = await httpClient.GetAsync("https://localhost:7160/Paquetes/Listado");
            if (image.IsSuccessStatusCode)
            {
                var content =  image.Content.ReadAsStringAsync().Result;
                var paquetes = JsonConvert.DeserializeObject<List<Paquete>>(content);
                int indice = paquetes.FindIndex(paquete => paquete.PaqueteID == id);
                ViewBag.image = indice+"Room.jpg";
                return View();

            }

            return RedirectToAction("Index");


        }

        [HttpPost]
        private decimal CalcularDescuento(int noches)
        {
            if (noches >= 3 && noches <= 6)
                return 10;
            else if (noches >= 7 && noches <= 9)
                return 15;
            else if (noches >= 10 && noches <= 12)
                return 20;
            else if (noches >= 13)
                return 25;
            else
                return 0; // Sin descuento
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> paquete([Bind] Reservacion reservacion, int id)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"/Paquetes/Buscar?id={id}");
            Paquete paquete = new Paquete();
            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                paquete = JsonConvert.DeserializeObject<Paquete>(resultado);
                ViewBag.paquete = paquete;

            }

            var image = await httpClient.GetAsync("https://localhost:7160/Paquetes/Listado");
            if (image.IsSuccessStatusCode)
            {
                var content = image.Content.ReadAsStringAsync().Result;
                var paquetes = JsonConvert.DeserializeObject<List<Paquete>>(content);
                int indice = paquetes.FindIndex(paquete => paquete.PaqueteID == id);
                ViewBag.image = indice + "Room.jpg";

            }


            // Total sin descuento
            reservacion.TotalSinDescuento = reservacion.Noches * reservacion.Personas * paquete.PrecioPorPersonaPorNoche;

            // Calcular porcentaje de descuento
            var descuentoPorcentaje = CalcularDescuento(reservacion.Noches);
            reservacion.DescuentoPorcentaje = descuentoPorcentaje;

            // Total con descuento
            reservacion.TotalConDescuento = reservacion.TotalSinDescuento * (1 - (descuentoPorcentaje / 100));

            // Agregar IVA
            const decimal IVA = 13;
            decimal totalFinalConIVA = reservacion.TotalConDescuento * (1 + IVA / 100);
            

            // Calcular otros valores
            reservacion.Prima = reservacion.TotalConDescuento * (paquete.PrimaPorcentaje / 100);
            reservacion.RestoEnMensualidades = (reservacion.TotalConDescuento - reservacion.Prima) / paquete.Mensualidades;

            var tipoCambio = await httpClient.GetAsync("https://apis.gometa.org/tdc/tdc.json");
            if (tipoCambio.IsSuccessStatusCode)
            {
                var content = await tipoCambio.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(content);
                reservacion.TipoCambio = data.venta;
            }


            reservacion.TotalUSD = totalFinalConIVA / reservacion.TipoCambio;
            int userId = int.Parse(User.FindFirstValue("UserId"));
            reservacion.ClienteID = userId;
            reservacion.ReservacionID = 2;
            reservacion.PaqueteID = id;



            TempData["TotalFinal"] = "El total seria de "+totalFinalConIVA;
           


            return View(reservacion);
            

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create ([Bind] Reservacion reservacion)
        {
            //El ID de la reservacion se genera de forma automatica
            reservacion.ReservacionID = 0;
            int userId = int.Parse(User.FindFirstValue("UserId"));
            reservacion.ClienteID = userId;



            //Se utiliza la API web  para almacenar los datos de la reservacion
            var agregar =  httpClient.PostAsJsonAsync<Reservacion>("/Reservaciones/Agregar", reservacion);

            await agregar;  //se espera que termine la transacción

            var resultado = agregar.Result;

            if (resultado.IsSuccessStatusCode) //si todo fue correcto
            {
                //se ubica al usuario dentro del listado de reservaciones
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Mensaje"] = "No se logró registrar la reservacion..";
                return View(reservacion);
            }

        }







    }
}
