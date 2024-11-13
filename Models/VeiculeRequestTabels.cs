using Microsoft.AspNetCore.Mvc;

namespace ProjectEstagio.Models
{
    public class VeiculeRequestTabels
    {
        public List<VeiculeRequest> FutureRequests { get; set; }
        public List<VeiculeRequest> PastRequests { get; set; }
        
    }
}
