using System.ComponentModel.DataAnnotations;

namespace ProjectEstagio.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Local { get; set; }
    }
}
