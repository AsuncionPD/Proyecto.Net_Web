namespace AppWebBeachSA.Models
{
    public class AutorizacionResponse
    {
        public string Token { get; set; }
        public string Msj { get; set; }
        public bool Resultado { get; set; }
        public int TipoUsuario { get; set; }
    }
}
