using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectEstagio.Models
{
    public class VeiculeRequestViewModel
    {
        [Required(ErrorMessage = "Por favor, selecione um veículo.")]
        [Display(Name = "Veículo")]
        public int VeiculeId { get; set; }

        [Display(Name = "Tipo de Veículo")]
        public string VeiculeType { get; set; }

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

        // Campo opcional para motorista
        [Display(Name = "Motorista")]
        public int? DriverId { get; set; }

        // Lista de veículos disponíveis para seleção
        public List<SelectListItem> Veicules { get; set; } = new List<SelectListItem>();

        // Lista de motoristas disponíveis para seleção (se aplicável)
        public List<SelectListItem> Drivers { get; set; } = new List<SelectListItem>();

        // Quilómetros utilizados após o uso do veículo
        [Display(Name = "Quilómetros Usados")]
        public int? KilometersUsed { get; set; }

        [MaxLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}
