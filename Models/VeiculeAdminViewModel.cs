namespace ProjectEstagio.Models
{
    public class VeiculeAdminViewModel
    {
        public List<Veicule> Veicules { get; set; } = new List<Veicule>();
        public List<Driver> Drivers { get; set; } = new List<Driver>();

        public VeiculeRequestViewModel VeiculeRequest { get; set; } = new VeiculeRequestViewModel();

    }
}
