using System.ComponentModel.DataAnnotations;

namespace AppWebBeachSA.Models
{
    public class Paquete
    {
        [Key]
        [Required]
        public int PaqueteID { get; set; }


        [Required]
        public string Nombre { get; set; }


        [Required]
        public decimal PrecioPorPersonaPorNoche { get; set; }


        [Required]
        public decimal PrimaPorcentaje { get; set; }


        [Required]
        public int Mensualidades { get; set; }

    }
}
