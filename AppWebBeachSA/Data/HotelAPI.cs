namespace AppWebBeachSA.Data
{
    public class HotelAPI
    {
        //Método encargado de inicializar la comunicación con la API
        public HttpClient Inicial()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://www.apibeachsa.somee.com");
            return client;
        }
    }
}
