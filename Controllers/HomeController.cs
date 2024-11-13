using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectEstágio.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProjectEstágio.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<string> GetUserRole()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return "Admin";
                }
                else if (await _userManager.IsInRoleAsync(user, "GestorSalas"))
                {
                    return "GestorSalas";
                }
                else if (await _userManager.IsInRoleAsync(user, "GestorVeiculos"))
                {
                    return "GestorVeiculos";
                }
                else if (await _userManager.IsInRoleAsync(user, "GestorEquipamentos"))
                {
                    return "GestorEquipamentos";
                }
                else
                {
                    return "Public"; // Ou qualquer nome que você deseje para usuários não autenticados
                }
            }
            return "Public"; // Se não houver usuário logado
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
