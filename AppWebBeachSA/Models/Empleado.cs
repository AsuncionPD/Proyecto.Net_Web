using System.ComponentModel.DataAnnotations;

namespace AppWebBeachSA.Models
{
    public class Empleado
    {
        [Key]
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [Required]
        public int TipoUsuario { get; set; }

     
        [Required]
        public string Nombre { get; set; }

        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        public bool Confirmar(string pw)
        {
            bool resultado = false;

            if (Password != null)
            {
                if (Password.Equals(pw))
                {
                    resultado = true;
                }

            }
            return resultado;
        }
    }
}
