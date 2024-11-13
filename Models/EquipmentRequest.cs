using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectEstagio.Models
{
    public class EquipmentRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EquipmentId { get; set; }

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

        // Propriedade de navegação para o equipamento
        public Equipment Equipment { get; set; }

    }
}
