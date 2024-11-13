using System.ComponentModel.DataAnnotations;

namespace ProjectEstagio.Models
{
    public class Driver
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Telefone { get; set; }
    }
}
