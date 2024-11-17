using System.ComponentModel.DataAnnotations;

namespace AppWebBeachSA.Models
{
    public class Reservacion
    {
        [Key]
        [Required]
        public int ReservacionID { get; set; }

        [Required]
        public int ClienteID { get; set; }

        [Required]
        public int PaqueteID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaReservacion { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad de noches debe ser al menos 1")]
        public int Noches { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad de personas debe ser al menos 1")]
        public int Personas { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalSinDescuento { get; set; }

        [Range(0, 100, ErrorMessage = "El porcentaje de descuento debe estar entre 0 y 100")]
        public decimal DescuentoPorcentaje { get; set; }

        [Required]
        public decimal TotalConDescuento { get; set; }

        [Required]
        public string FormaPago { get; set; }

        public string NumeroCheque { get; set; } 
        public string Banco { get; set; } 

        [Required]
        [DataType(DataType.Currency)]
        public decimal Prima { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal RestoEnMensualidades { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El tipo de cambio debe ser positivo")]
        public decimal TipoCambio { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalUSD { get; set; }
    }
}
