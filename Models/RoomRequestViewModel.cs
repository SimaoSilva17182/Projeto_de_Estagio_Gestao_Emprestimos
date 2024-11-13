using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectEstagio.Models
{
    public class RoomRequestViewModel
    {
        [Required(ErrorMessage = "Por favor, selecione uma sala.")]
        [Display(Name = "Sala")]
        public int RoomId { get; set; }
        [Required(ErrorMessage = "Por favor, selecione uma data.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Por favor, insira a hora de início.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Início")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Por favor, insira a hora de término.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Término")]
        public TimeSpan EndTime { get; set; }

        public List<SelectListItem> Rooms { get; set; } = new List<SelectListItem>();
    }
}
