using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectEstágio.Data;
using ProjectEstágio.Models;
using ProjectEstagio.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace ProjectEstagio.Controllers
{
    public class VeiculeController : Controller
    {
        private readonly ILogger<VeiculeController> _logger;
        private readonly ApplicationDbContext _context;

        public VeiculeController(ILogger<VeiculeController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        // Método para exibir a página de administração de veículos
        [Authorize(Roles = "Admin")]
        public IActionResult VeiculeAdmin()
        {
            // Carregar veículos e motoristas da base de dados
            var veicules = _context.Veicules.ToList();
            var drivers = _context.Drivers.ToList();

            // Criar um modelo para a view
            var model = new VeiculeAdminViewModel
            {
                Veicules = veicules,
                Drivers = drivers,
                VeiculeRequest = new VeiculeRequestViewModel
                {
                    Veicules = veicules.Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.Name} ({v.Plate})"
                    }).ToList()
                }
            };

            return View(model); // Retorna o modelo para a view
        }

        [Authorize(Roles = "Admin,GestorVeiculos")]
        public IActionResult VeiculeRequest()
        {
            // Carregar os veículos da base de dados
            var veicules = _context.Veicules.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.Name} ({v.Plate}) - Tipo: {v.Type}", // Exibe o nome do veículo e a matrícula (Plate)
            }).ToList();

            // Carregar os motoristas da base de dados
            var drivers = _context.Drivers.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = $"{d.Name} - {d.Telefone}" // Exibe o nome do motorista
            }).ToList();

            // Inicializar o viewmodel
            var model = new VeiculeRequestViewModel
            {
                Veicules = veicules,
                Drivers = drivers
            };

            return View(model);
        }

        // Método para criar um novo veículo com suporte a upload de imagem
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateVeicule(string name, string type, string plate, IFormFile imageFile)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(plate))
            {
                string imagePath = "/images/default-veicule.png"; // Caminho padrão para a imagem

                // Verifica se um arquivo de imagem foi carregado
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images"); // Caminho onde as imagens serão guardadas
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName; // Nome único para evitar conflitos
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Salva o arquivo no servidor
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    imagePath = "/images/" + uniqueFileName; // Atualiza o caminho da imagem para o novo arquivo
                }

                // Criação do veículo com a imagem (ou imagem padrão se não fornecida)
                var veicule = new Veicule { Name = name, Type = type, Plate = plate, Image = imagePath };
                _context.Veicules.Add(veicule);
                await _context.SaveChangesAsync();

                return RedirectToAction("VeiculeAdmin");
            }

            return BadRequest();
        }

        // Método para adicionar um motorista
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateDriver(string name, string telefone)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(telefone))
            {
                var driver = new Driver { Name = name, Telefone = telefone };
                _context.Drivers.Add(driver);
                _context.SaveChanges();
                return RedirectToAction("VeiculeAdmin");
            }
            return BadRequest();
        }

        // Método para remover um veículo
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteVeicule(int id)
        {
            var veicule = _context.Veicules.Find(id);
            if (veicule != null)
            {
                _context.Veicules.Remove(veicule);
                _context.SaveChanges();
                return RedirectToAction("VeiculeAdmin");
            }
            return NotFound();
        }

        // Método para remover um motorista
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteDriver(int id)
        {
            var driver = _context.Drivers.Find(id);
            if (driver != null)
            {
                _context.Drivers.Remove(driver);
                _context.SaveChanges();
                return RedirectToAction("VeiculeAdmin");
            }
            return NotFound();
        }

        // Método para remover pedido de veículo
        [HttpPost]
        [Authorize(Roles = "Admin,GestorVeiculos")]
        public IActionResult DeleteVeiculeRequest(int id)
        {
            var request = _context.VeiculeRequests.Find(id);
            if (request != null)
            {
                _context.VeiculeRequests.Remove(request);
                _context.SaveChanges();
                return RedirectToAction("MyVeiculeRequests");
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "GestorVeiculos,Admin")]
        public IActionResult CreateVeiculeRequest(VeiculeRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Carrega o veículo selecionado para obter o tipo
                var selectedVeicule = _context.Veicules.FirstOrDefault(v => v.Id == model.VeiculeId);

                if (selectedVeicule != null)
                {
                    model.VeiculeType = selectedVeicule.Type; // Popula o tipo do veículo no ViewModel
                }

                // Cria a data e hora de início e término antes da consulta
                var startDateTime = new DateTime(model.Date.Year, model.Date.Month, model.Date.Day, model.StartTime.Hours, model.StartTime.Minutes, 0);
                var endDateTime = new DateTime(model.EndDate.Year, model.EndDate.Month, model.EndDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 0);

                // Valida se a hora de término é posterior à de início
                if (model.Date == model.EndDate && endDateTime <= startDateTime)
                {
                    ModelState.AddModelError(string.Empty, "A hora de término deve ser posterior à hora de início.");
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var existingUser = _context.Users.SingleOrDefault(u => u.Id == userId);

                    if (existingUser == null)
                    {
                        ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                    }
                    else
                    {
                        // Verifica se o tipo de veículo requer motorista
                        if ((selectedVeicule.Type == "carrinha" || selectedVeicule.Type == "autocarro") && model.DriverId == null)
                        {
                            ModelState.AddModelError(string.Empty, "É necessário selecionar um motorista para carrinhas ou autocarros.");
                        }
                        else
                        {
                            // Criação da requisição
                            var veiculeRequest = new VeiculeRequest
                            {
                                VeiculeId = model.VeiculeId,
                                Date = model.Date,
                                EndDate = model.EndDate,
                                StartTime = startDateTime.TimeOfDay,
                                EndTime = endDateTime.TimeOfDay,
                                UserId = userId,
                                DriverId = model.DriverId, // Pode ser null para veículos "ligeiros"
                                Description = model.Description
                            };

                            //Carrega todas as requisições de Veiculos para memória
                            var existingRequests = _context.VeiculeRequests
                                .Where(er => er.VeiculeId == veiculeRequest.VeiculeId)
                                .ToList();

                            // Verifica conflitos de horário veiculos
                            var conflict = existingRequests.Any(er =>
                            (
                                // Caso 1: A data de início e a data final do novo pedido estão dentro de um período existente
                                (model.Date >= er.Date && model.Date <= er.EndDate) ||
                                (model.EndDate >= er.Date && model.EndDate <= er.EndDate) ||

                                // Caso 2: O período de datas do novo pedido engloba um período já existente
                                (model.Date <= er.Date && model.EndDate >= er.EndDate)
                            ) ||
                            (
                                // Verificar sobreposição de horas para a mesma data de início e data final
                                (
                                    (model.Date == er.Date && model.StartTime < er.EndTime && model.EndTime > er.StartTime) ||
                                    (model.EndDate == er.EndDate && model.StartTime < er.EndTime && model.EndTime > er.StartTime)
                                )
                            ) && (
                                er.VeiculeId == veiculeRequest.VeiculeId || er.DriverId == veiculeRequest.DriverId // Mesmo veículo ou motorista
                            ));

                            if (conflict)
                            {
                                ModelState.AddModelError(string.Empty, "O horário solicitado conflita com uma reserva existente.");
                            }
                            else
                            {
                                _context.VeiculeRequests.Add(veiculeRequest);
                                _context.SaveChanges();

                                // Redireciona com base na role
                                return User.IsInRole("GestorVeiculos") ? RedirectToAction("VeiculeRequest") : RedirectToAction("VeiculeRequest");

                            }
                        }
                    }
                }
            }

            // Preencher novamente os veículos e motoristas no ViewModel em caso de erro
            model.Veicules = _context.Veicules.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.Name} ({v.Plate} - Tipo: {v.Type})"
            }).ToList();

            model.Drivers = _context.Drivers.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList();

            return View("VeiculeRequest", model);
        }

        [HttpPost]
        [Authorize]
        public IActionResult UpdateKilometers(int requestId, int kilometersUsed)
        {
            // Encontra o pedido de veículo específico
            var veiculeRequest = _context.VeiculeRequests
                .Include(v => v.Veicule)
                .FirstOrDefault(v => v.Id == requestId);

            if (veiculeRequest == null)
            {
                return NotFound(); // Pedido não encontrado
            }

            // Atualiza os quilómetros usados no pedido e no veículo
            veiculeRequest.KilometersUsed = kilometersUsed;
            veiculeRequest.Veicule.InitialKilometers += kilometersUsed;

            // Salva as alterações no banco de dados
            _context.SaveChanges();

            return RedirectToAction("MyVeiculeRequests"); // Redireciona para a página de pedidos
        }


        [Authorize]
        public IActionResult MyVeiculeRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var today = DateTime.Now.Date;

            // Requisitos futuros
            List<VeiculeRequest> futureRequests;
            List<VeiculeRequest> pastRequests;

            if (isAdmin)
            {
                futureRequests = _context.VeiculeRequests
                    .Where(r => r.Date >= today)
                    .Include(r => r.Veicule)
                    .Include(r => r.Driver)
                    .Include(r => r.User)
                    .OrderBy(r => r.Date)
                    .ToList();

                pastRequests = _context.VeiculeRequests
                    .Where(r => r.Date < today)
                    .Include(r => r.Veicule)
                    .Include (r => r.Driver)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.Date)
                    .ToList();
            }
            else
            {
                futureRequests = _context.VeiculeRequests
                    .Where(r => r.UserId == userId && r.Date >= today)
                    .Include(r => r.Veicule)
                    .Include(r => r.Driver)
                    .OrderBy(r => r.Date)
                    .ToList();

                pastRequests = _context.VeiculeRequests
                    .Where(r => r.UserId == userId && r.Date < today)
                    .Include(r => r.Veicule)
                    .Include(r => r.Driver)
                    .OrderByDescending(r => r.Date)
                    .ToList();
            }

            var model = new VeiculeRequestTabels
            {
                FutureRequests = futureRequests,
                PastRequests = pastRequests
            };

            return View(model);
        }

        [Authorize]
        public IActionResult VeiculeAvailability()
        {
            var model = new VeiculeAvailabilityViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public IActionResult CheckVeiculeAvailability(VeiculeAvailabilityViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Cria a data e hora de início e término antes da consulta
                var startDateTime = new DateTime(model.Date.Year, model.Date.Month, model.Date.Day, model.StartTime.Hours, model.StartTime.Minutes, 0);
                var endDateTime = new DateTime(model.EndDate.Year, model.EndDate.Month, model.EndDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 0);

                // Verifica se a hora de término é posterior à hora de início
                if (model.Date == model.EndDate && endDateTime <= startDateTime)
                {
                    ModelState.AddModelError(string.Empty, "A hora de término deve ser posterior à hora de início.");
                    return View("VeiculeAvailability", model);
                }

                // Verificar se há veículos já reservados neste horário
                var bookedVeicules = _context.VeiculeRequests
                    .Where(vr =>
                        (
                            // Verifica se o intervalo solicitado está dentro de um período existente
                            (startDateTime >= vr.Date && startDateTime <= vr.EndDate) ||
                            (endDateTime >= vr.Date && endDateTime <= vr.EndDate) ||
                            // Verifica se o intervalo solicitado engloba um período existente
                            (startDateTime <= vr.Date && endDateTime >= vr.EndDate)
                        ) &&
                        (
                            // Verifica a sobreposição de horários
                            (vr.Date == model.Date && model.StartTime < vr.EndTime && model.EndTime > vr.StartTime) ||
                            (vr.EndDate == model.EndDate && model.StartTime < vr.EndTime && model.EndTime > vr.StartTime)
                        )
                    )
                    .Select(vr => vr.VeiculeId)
                    .ToList();

                // Filtro de veículos disponíveis com base no tipo selecionado
                var availableVeiculesQuery = _context.Veicules
                    .Where(v => !bookedVeicules.Contains(v.Id));  // Exclui os veículos já reservados

                // Aplica o filtro de tipo de veículo se não for "todos"
                if (model.VeiculeType != "todos")
                {
                    availableVeiculesQuery = availableVeiculesQuery.Where(v => v.Type == model.VeiculeType);
                }

                // Listar veículos disponíveis
                model.AvailableVeicules = availableVeiculesQuery
                    .Select(v => new VeiculeAvailabilityItem
                    {
                        Name = v.Name,
                        Type = v.Type,
                        Image = v.Image ?? "/images/default-veicule.png" // Usa imagem padrão se não houver uma definida
                    })
                    .ToList();

                // Define a flag indicando que uma pesquisa foi realizada
                ViewData["HasSearched"] = true;

                // Se não houver veículos disponíveis, exibe a mensagem
                if (!model.AvailableVeicules.Any())
                {
                    ModelState.AddModelError(string.Empty, "Não há veículos disponíveis para o horário e tipo selecionado.");
                }

                return View("VeiculeAvailability", model);
            }

            // Redefine StartTime e EndTime para nulo
            model.StartTime = TimeSpan.Zero;
            model.EndTime = TimeSpan.Zero;

            return View("VeiculeAvailability", model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
