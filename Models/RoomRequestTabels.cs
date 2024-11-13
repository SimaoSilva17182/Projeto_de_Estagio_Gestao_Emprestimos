using Microsoft.AspNetCore.Mvc;

namespace ProjectEstagio.Models
{
    public class RoomRequestTabels : Controller
    {
        public List<RoomRequest> FutureRequests { get; set; } = new List<RoomRequest>();
        public List<RoomRequest> PastRequests { get; set;} = new List<RoomRequest>();
    }
}
