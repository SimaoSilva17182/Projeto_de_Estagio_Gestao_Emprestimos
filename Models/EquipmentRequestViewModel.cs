using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectEstagio.Models
{
    public class EquipmentRequestViewModel
    {
        [Required(ErrorMessage = "Por favor, selecione um equipamento.")]
        [Display(Name = "Equipamento")]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "Por favor, selecione uma data.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Por favor, selecione uma data.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data Final")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Por favor, insira a hora de início.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Início")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Por favor, insira a hora de término.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Término")]
        public TimeSpan EndTime { get; set; }

        public List<SelectListItem> Equipments { get; set; } = new List<SelectListItem>();

    }
}
