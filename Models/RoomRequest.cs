using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectEstagio.Models
{
    public class RoomRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

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

        // Navigation Property
        public Room Room { get; set; }
    }
}


