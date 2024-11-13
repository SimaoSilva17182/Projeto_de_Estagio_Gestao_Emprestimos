namespace ProjectEstagio.Models
{
    public class EquipmentAdminViewModel
    {
        public List<Equipment> Equipments { get; set; }
        public EquipmentRequestViewModel EquipmentRequest { get; set; }

        public EquipmentAdminViewModel()
        {
            Equipments = new List<Equipment>();
            EquipmentRequest = new EquipmentRequestViewModel();
        }
    }
}
