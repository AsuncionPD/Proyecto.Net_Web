using System.ComponentModel.DataAnnotations;

namespace AppWebBeachSA.Models
{
    public class Paquete
    {
        [Key]
        [Required]
        [Display(Name = "Package ID")]
        public int PaqueteID { get; set; }


        [Required]
        [Display(Name = "Name")]
        public string Nombre { get; set; }


        [Required]
        [Display(Name = "Pricer per Person")]
        public decimal PrecioPorPersonaPorNoche { get; set; }


        [Required]
        [Display(Name = "Down Payment Percentage")]
        public decimal PrimaPorcentaje { get; set; }


        [Required]
        [Display(Name = "Monthly Payment")]
        public int Mensualidades { get; set; }

    }
}
