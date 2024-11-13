using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectEstagio.Models
{
    public class VeiculeAvailabilityViewModel
    {
       
        [Required(ErrorMessage = "Por favor, insira a data.")]
        [Display(Name = "Data Inicial")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Por favor, insira a data.")]
        [Display(Name = "Data Final")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Por favor, insira a hora de início.")]
        [Display(Name = "Hora de Início")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Por favor, insira a hora de término.")]
        [Display(Name = "Hora de Término")]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Tipo de Veículo")]
        public string VeiculeType { get; set; } = "todos";

        public List<VeiculeAvailabilityItem> AvailableVeicules { get; set; } = new List<VeiculeAvailabilityItem>();

        public List<SelectListItem> VeiculeTypeOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "todos", Text = "Todos" },
            new SelectListItem { Value = "ligeiro", Text = "Ligeiro" },
            new SelectListItem { Value = "carrinha", Text = "Carrinha" },
            new SelectListItem { Value = "autocarro", Text = "Autocarro" }
        };
    }

    public class VeiculeAvailabilityItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
    }
}
