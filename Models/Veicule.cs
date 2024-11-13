using System.ComponentModel.DataAnnotations;

namespace ProjectEstagio.Models
{
    public class Veicule
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; } // ligeiro, carrinha, autocarro
        [Required]
        public string Plate { get; set; }
        [Required]
        public int InitialKilometers { get; set; } // Quilómetros iniciais
        public string Image { get; set; }
        
    }
}
