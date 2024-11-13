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
    public class EquipmentController : Controller
    {
        private readonly ILogger<EquipmentController> _logger;
        private readonly ApplicationDbContext _context;

        public EquipmentController(ILogger<EquipmentController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Método para exibir a página de administração
        [Authorize(Roles = "Admin")]
        public IActionResult EquipmentAdmin()
        {
            // Carregar os equipamentos da base de dados
            var equipments = _context.Equipments.ToList();

            // Criar um modelo para a view
            var model = new EquipmentAdminViewModel
            {
                Equipments = equipments,
                EquipmentRequest = new EquipmentRequestViewModel
                {
                    Equipments = equipments.Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = $"{e.Name} - {e.Type}"
                    }).ToList()
                }
            };

            return View(model); // Retorna o modelo para a view
        }

        // Método para criar um novo equipamento
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateEquipment(string name, string type)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type))
            {
                var equipment = new Equipment { Name = name, Type = type };
                _context.Equipments.Add(equipment);
                _context.SaveChanges();
                return RedirectToAction("EquipmentAdmin"); // Redireciona para a página de administração
            }
            return BadRequest(); // Retorna erro caso o nome ou tipo seja inválido
        }

        // Método para remover um equipamento
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteEquipment(int id)
        {
            var equipment = _context.Equipments.Find(id);
            if (equipment != null)
            {
                _context.Equipments.Remove(equipment);
                _context.SaveChanges();
                return RedirectToAction("EquipmentAdmin"); // Redireciona para a página de administração
            }
            return NotFound(); // Retorna erro caso o equipamento não seja encontrado
        }

        // Método para remover pedido de equipamento
        [HttpPost]
        [Authorize(Roles = "Admin,GestorSalas")]
        public IActionResult DeleteEquipmentRequest(int id)
        {
            var request = _context.EquipmentRequests.Find(id);
            if (request != null)
            {
                _context.EquipmentRequests.Remove(request);
                _context.SaveChanges();
                return RedirectToAction("MyEquipmentRequests");
            }
            return NotFound();
        }

        [Authorize(Roles = "Admin,GestorEquipamentos")]
        public IActionResult EquipmentRequest()
        {
            // Carregar os equipamentos da base de dados
            var equipments = _context.Equipments.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.Name} - {e.Type}"
            }).ToList();

            // Inicializar o viewmodel
            var model = new EquipmentRequestViewModel
            {
                Equipments = equipments
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "GestorEquipamentos,Admin")]
        public IActionResult CreateEquipmentRequest(EquipmentRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Cria a data e hora de início e término antes da consulta
                var startDateTime = new DateTime(model.Date.Year, model.Date.Month, model.Date.Day, model.StartTime.Hours, model.StartTime.Minutes, 0); 
                var endDate = model.EndDate;
                var endDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, model.EndTime.Hours, model.EndTime.Minutes, 0);


                // Valida se a hora de término é posterior à de início
                if (model.Date == model.EndDate && endDateTime <= startDateTime)
                {
                    ModelState.AddModelError(string.Empty, "A hora de término deve ser posterior à hora de início.");
                }
                else
                {
                    // Verifica se o usuário existe usando o Claim para UserId
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var existingUser = _context.Users.SingleOrDefault(u => u.Id == userId);

                    if (existingUser == null)
                    {
                        ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                    }
                    else
                    {
                        // Criação da requisição
                        var equipmentRequest = new EquipmentRequest
                        {
                            EquipmentId = model.EquipmentId,
                            Date = model.Date,
                            EndDate = model.EndDate,
                            StartTime = startDateTime.TimeOfDay,
                            EndTime = endDateTime.TimeOfDay,
                            UserId = userId
                        };

                        // Carrega todas as requisições de equipamentos para memória
                        var existingRequests = _context.EquipmentRequests
                            .Where(er => er.EquipmentId == equipmentRequest.EquipmentId)
                            .ToList();

                        // Verifica conflitos de horário na memória
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
                            )
                        );

                        // Se houver conflito, adiciona erro ao ModelState
                        if (conflict)
                        {
                            ModelState.AddModelError(string.Empty, "O horário solicitado conflita com um empréstimo existente.");
                        }
                        else
                        {
                            // Adiciona a requisição ao contexto e salva
                            _context.EquipmentRequests.Add(equipmentRequest);
                            _context.SaveChanges();

                            // Redireciona com base na role
                            return User.IsInRole("GestorEquipamentos") ? RedirectToAction("EquipmentRequest") : RedirectToAction("EquipmentRequest");
                        }
                    }
                }
            }

            // Repopula a lista de equipamentos no caso de erro
            var equipments = _context.Equipments.ToList();
            model.Equipments = equipments.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.Name} - {e.Type}"
            }).ToList();

            // Redefine StartTime e EndTime para nulo ou um valor que acione a opção "Selecione..."
            model.StartTime = TimeSpan.Zero;
            model.EndTime = TimeSpan.Zero;

            return View("EquipmentRequest", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateEquipmentRequestAdmin(EquipmentAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Cria a data e hora de início e término antes da consulta
                var startDateTime = new DateTime(model.EquipmentRequest.Date.Year, model.EquipmentRequest.Date.Month, model.EquipmentRequest.Date.Day, model.EquipmentRequest.StartTime.Hours, model.EquipmentRequest.StartTime.Minutes, 0);

                // Verifica se há uma data final fornecida
                var endDate = model.EquipmentRequest.EndDate;

                // Cria a data e hora de término antes da consulta (pode ser uma data diferente)
                var endDateTime = new DateTime(
                    endDate.Year,
                    endDate.Month,
                    endDate.Day,
                    model.EquipmentRequest.EndTime.Hours,
                    model.EquipmentRequest.EndTime.Minutes,
                    0
                );

                // Valida se a hora de término é posterior à de início
                if (model.EquipmentRequest.Date == model.EquipmentRequest.EndDate && endDateTime <= startDateTime)
                {
                    ModelState.AddModelError(string.Empty, "A data e hora de término devem ser posteriores à data e hora de início.");
                }
                else
                {
                    // Verifica se o usuário existe usando o Claim para UserId
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var existingUser = _context.Users.SingleOrDefault(u => u.Id == userId);

                    if (existingUser == null)
                    {
                        ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                    }
                    else
                    {
                        // Criação da requisição
                        var equipmentRequest = new EquipmentRequest
                        {
                            EquipmentId = model.EquipmentRequest.EquipmentId,
                            Date = model.EquipmentRequest.Date,
                            EndDate = model.EquipmentRequest.EndDate,
                            StartTime = startDateTime.TimeOfDay,
                            EndTime = endDateTime.TimeOfDay,
                            UserId = userId
                        };

                        // Carrega todas as requisições de equipamentos para memória
                        var existingRequests = _context.EquipmentRequests
                            .Where(er => er.EquipmentId == equipmentRequest.EquipmentId)
                            .ToList();

                        // Verifica conflitos de horário na memória
                        var conflict = existingRequests.Any(er =>
                            (
                                // Caso 1: A data de início e a data final do novo pedido estão dentro de um período existente
                                (equipmentRequest.Date >= er.Date && equipmentRequest.Date <= er.EndDate) ||
                                (equipmentRequest.EndDate >= er.Date && equipmentRequest.EndDate <= er.EndDate) ||

                                // Caso 2: O período de datas do novo pedido engloba um período já existente
                                (equipmentRequest.Date <= er.Date && equipmentRequest.EndDate >= er.EndDate)
                            ) &&
                                (
                                // Verificar sobreposição de horas para a mesma data de início e data final
                                (
                                    (equipmentRequest.Date == er.Date && equipmentRequest.StartTime < er.EndTime && equipmentRequest.EndTime > er.StartTime) ||
                                    (equipmentRequest.EndDate == er.EndDate && equipmentRequest.StartTime < er.EndTime && equipmentRequest.EndTime > er.StartTime)
                                )
                            )
                        );

                        // Se houver conflito, adiciona erro ao ModelState
                        if (conflict)
                        {
                            ModelState.AddModelError(string.Empty, "O horário solicitado conflita com um empréstimo existente.");
                        }
                        else
                        {
                            // Adiciona a requisição ao contexto e salva
                            _context.EquipmentRequests.Add(equipmentRequest);
                            _context.SaveChanges();

                            // Redireciona com base na role
                            return User.IsInRole("Admin") ? RedirectToAction("EquipmentAdmin") : RedirectToAction("EquipmentRequest");
                        }
                    }
                }
            }

            // Se houver erros, repopula o modelo e retorna à view correta
            var equipments = _context.Equipments.ToList();
            model.EquipmentRequest.Equipments = equipments.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.Name} - {e.Type}"
            }).ToList();

            // Redefine StartTime e EndTime para nulo ou um valor que acione a opção "Selecione..."
            model.EquipmentRequest.StartTime = TimeSpan.Zero;
            model.EquipmentRequest.EndTime = TimeSpan.Zero;

            return View("EquipmentAdmin", model);
        }

        [Authorize]
        public IActionResult MyEquipmentRequests()
        {
            // Obtém o UserId do usuário logado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verifica se o usuário é administrador
            var isAdmin = User.IsInRole("Admin");

            // Define a data atual
            var today = DateTime.Now.Date;

            // Busca os requisitos com base no papel do usuário
            List<EquipmentRequest> futureRequests;
            List<EquipmentRequest> pastRequests;

            if (isAdmin)
            {
                // Se for administrador, busca todos os futuros requisitos
                futureRequests = _context.EquipmentRequests
                    .Where(r => r.Date >= today)
                    .Include(r => r.Equipment)
                    .Include(r => r.User)
                    .OrderBy(r => r.Date)
                    .ThenBy(r => r.StartTime) // ordena por hora de inicio
                    .ToList();

                // busca os pedidos anterioes
                pastRequests = _context.EquipmentRequests
                     .Where(d => d.Date < today)
                     .Include(d => d.Equipment)
                     .Include(d => d.User)
                     .OrderByDescending(d => d.Date) // ordena por ordem decrecente
                     .ThenBy(d  => d.StartTime)
                     .ToList();
            }
            else
            {
                // Se não for administrador, busca apenas os requisitos do usuário logado
                futureRequests = _context.EquipmentRequests
                    .Where(r => r.UserId == userId && r.Date >= today)
                    .Include(r => r.Equipment)
                    .OrderBy (r => r.Date)
                    .ThenBy(r => r.StartTime)
                    .ToList();

                pastRequests = _context.EquipmentRequests
                   .Where(r => r.UserId == userId && r.Date < today)
                   .Include(r => r.Equipment) // Inclui dados dos equipamentos associadas
                   .OrderByDescending(r => r.Date)
                   .ThenBy (r => r.StartTime)
                   .ToList();
            }

            // Retorna as listas para a view, por meio de um novo ViewModel
            var model = new EquipmentRequestTabels
            {
                FutureRequests = futureRequests,
                PastRequests = pastRequests
            };

            return View(model);
        }

        [Authorize]
        public IActionResult EquipmentAvailability()
        {
            var model = new EquipmentAvailabilityViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public IActionResult CheckEquipmentAvailability(EquipmentAvailabilityViewModel model)
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
                    return View("EquipmentAvailability", model);
                }

                // Verificar se há equipamentos já reservadas neste horário
                var bookedEquipments = _context.EquipmentRequests
                    .Where(r =>
                        (
                            // Caso 1: O intervalo solicitado está dentro de um período existente
                            (startDateTime >= r.Date && startDateTime <= r.EndDate) ||
                            (endDateTime >= r.Date && endDateTime <= r.EndDate) ||
                            // Caso 2: O intervalo solicitado engloba um período existente
                            (startDateTime <= r.Date && endDateTime >= r.EndDate)
                        ) &&
                        (
                            // Verificar sobreposição de horários para as mesmas datas
                            (r.Date == model.Date && model.StartTime < r.EndTime && model.EndTime > r.StartTime) ||
                            (r.EndDate == model.EndDate && model.StartTime < r.EndTime && model.EndTime > r.StartTime)
                        )
                    )
                    .Select(r => r.EquipmentId)
                    .ToList();

                // Listar equipamentos disponíveis
                model.AvailableEquipments = _context.Equipments
                    .Where(r => !bookedEquipments.Contains(r.Id))
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = $"{r.Name} - {r.Type}"
                    })
                    .ToList();

                // Define a flag indicando que uma pesquisa foi realizada
                ViewData["HasSearched"] = true;

                // Se não houver equipamentos disponíveis, exibe a mensagem
                if (!model.AvailableEquipments.Any())
                {
                    ModelState.AddModelError(string.Empty, "Não há equipamentos disponíveis para o horário selecionado.");
                }

                return View("EquipmentAvailability", model);
            }
            // Redefine StartTime e EndTime para nulo ou um valor que acione a opção "Selecione..."
            model.StartTime = TimeSpan.Zero;
            model.EndTime = TimeSpan.Zero;
            return View("EquipmentAvailability", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
