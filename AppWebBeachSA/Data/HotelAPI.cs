namespace AppWebBeachSA.Data
{
    public class HotelAPI
    {
        //Método encargado de inicializar la comunicación con la API
        public HttpClient Inicial()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7160");
            return client;
        }
    }
}
