namespace ProjectEstagio.Models
{
    public class RoomAdminViewModel
    {
        public List<Room> Rooms { get; set; } = new List<Room>();
        public RoomRequestViewModel RoomRequest { get; set; } = new RoomRequestViewModel();
    }
}
