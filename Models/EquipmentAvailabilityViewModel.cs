using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectEstagio.Models
{
    public class EquipmentAvailabilityViewModel
    {
        [Required(ErrorMessage = "Por favor, insira a data.")]
        [Display(Name = "Data Inicial")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Por favor, insira a data.")]
        [Display(Name = "Data Final")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Por favor, insira a hora de início.")]
        [Display(Name = "Hora de Início")]
        public TimeSpan StartTime { get; set; }  // Alterado para TimeSpan

        [Required(ErrorMessage = "Por favor, insira a hora de término.")]
        [Display(Name = "Hora de Término")]
        public TimeSpan EndTime { get; set; }  // Alterado para TimeSpan

        public List<SelectListItem> AvailableEquipments { get; set; } = new List<SelectListItem>();

    }
}
