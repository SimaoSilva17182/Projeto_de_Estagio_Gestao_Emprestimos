using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectEstagio.Models
{
    public class VeiculeRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VeiculeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        // UserId para associar o usuário à requisição
        [Required]
        public string UserId { get; set; }

        // Propriedade de navegação para o usuário
        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        // Propriedade de navegação para o veículo
        [ForeignKey("VeiculeId")]
        public Veicule Veicule { get; set; }

        // Campo opcional para motorista
        public int? DriverId { get; set; }

        // Propriedade de navegação para o motorista (opcional)
        [ForeignKey("DriverId")]
        public Driver Driver { get; set; }

        public int? KilometersUsed { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}
