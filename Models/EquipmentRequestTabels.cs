using Microsoft.AspNetCore.Mvc;

namespace ProjectEstagio.Models
{
    public class EquipmentRequestTabels : Controller
    {
        public List<EquipmentRequest> FutureRequests { get; set; } = new List<EquipmentRequest>();
        public List<EquipmentRequest> PastRequests { get; set; } = new List<EquipmentRequest>();
    }
}
