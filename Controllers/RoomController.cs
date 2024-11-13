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
    public class RoomController : Controller
    {
        private readonly ILogger<RoomController> _logger;
        private readonly ApplicationDbContext _context;

        public RoomController(ILogger<RoomController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Método para exibir a página de administração
        [Authorize(Roles = "Admin")]
        public IActionResult RoomAdmin()
        {
            // Carregar as salas da base de dados
            var rooms = _context.Rooms.ToList();

            // Criar um modelo para a view
            var model = new RoomAdminViewModel
            {
                Rooms = rooms,
                RoomRequest = new RoomRequestViewModel
                {
                    // Inicializa a lista de salas como SelectListItem
                    Rooms = rooms.Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.Name
                    }).ToList()
                }
            };

            return View(model); // Retorna o modelo para a view
        }


        // Método para criar uma nova sala
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateRoom(string name, string local)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(local))
            {
                var room = new Room { Name = name, Local = local };
                _context.Rooms.Add(room);
                _context.SaveChanges();
                return RedirectToAction("RoomAdmin"); // Redireciona para a página de administração
            }
            return BadRequest(); // Retorna erro caso o nome seja inválido
        }

        // Método para remover uma sala
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteRoom(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                _context.SaveChanges();
                return RedirectToAction("RoomAdmin"); // Redireciona para a página de administração
            }
            return NotFound(); // Retorna erro caso a sala não seja encontrada
        }

        // Método para remover pedido de sala
        [HttpPost]
        [Authorize(Roles = "Admin,GestorSalas")]
        public IActionResult DeleteRoomRequest(int id)
        {
            var request = _context.RoomRequests.Find(id);
            if (request != null)
            {
                _context.RoomRequests.Remove(request);
                _context.SaveChanges();
                return RedirectToAction("MyRoomRequests");
            }
            return NotFound();
        }

        [Authorize(Roles = "Admin,GestorSalas")]
        public IActionResult RoomRequest()
        {
            //carregar as salas da base de dados
            var rooms = _context.Rooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name,
            }).ToList();

            //inicializar o viewmodel
            var model = new RoomRequestViewModel
            {
                Rooms = rooms
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "GestorSalas,Admin")]
        public IActionResult CreateRoomRequest(RoomRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Cria a data e hora de início e término antes da consulta
                var startDateTime = new DateTime(model.Date.Year, model.Date.Month, model.Date.Day, model.StartTime.Hours, model.StartTime.Minutes, 0);
                var endDateTime = new DateTime(model.Date.Year, model.Date.Month, model.Date.Day, model.EndTime.Hours, model.EndTime.Minutes, 0);

                // Valida se a hora de término é posterior à de início
                if (endDateTime <= startDateTime)
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
                        var roomRequest = new RoomRequest
                        {
                            RoomId = model.RoomId,
                            Date = model.Date,
                            StartTime = startDateTime.TimeOfDay, // Guardar apenas TimeSpan
                            EndTime = endDateTime.TimeOfDay, // Guardar apenas TimeSpan
                            UserId = userId // Identificador do usuário
                        };

                        // Carrega todas as requisições de sala para memória
                        var existingRequests = _context.RoomRequests
                            .Where(rr => rr.RoomId == roomRequest.RoomId)
                            .ToList(); // Traz todos os pedidos da sala para a memória

                        // Verifica conflitos de horário na memória
                        var conflict = existingRequests.Any(rr =>
                            (startDateTime >= rr.Date.Date + rr.StartTime && startDateTime < rr.Date.Date + rr.EndTime) ||
                            (endDateTime > rr.Date.Date + rr.StartTime && endDateTime <= rr.Date.Date + rr.EndTime) ||
                            (startDateTime <= rr.Date.Date + rr.StartTime && endDateTime >= rr.Date.Date + rr.EndTime));

                        // Se houver conflito, adiciona erro ao ModelState
                        if (conflict)
                        {
                            ModelState.AddModelError(string.Empty, "O horário solicitado conflita com uma reserva existente.");
                        }
                        else
                        {
                            // Adiciona a requisição ao contexto e salva
                            _context.RoomRequests.Add(roomRequest);
                            _context.SaveChanges();

                            // Redireciona com base na role
                            return User.IsInRole("GestorSalas") ? RedirectToAction("RoomRequest") : RedirectToAction("RoomRequest");
                        }
                    }
                }
            }

            // Se houver erros, repopula o modelo e retorna à view correta
            var rooms = _context.Rooms.ToList();
            model.Rooms = rooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList();

            // Redefine StartTime e EndTime para nulo ou um valor que acione a opção "Selecione..."
            model.StartTime = TimeSpan.Zero;
            model.EndTime = TimeSpan.Zero;

            // Em caso de erro, retorna a view com o modelo
            return View("RoomRequest", model);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateRoomRequestAdmin(RoomAdminViewModel model)
        {           
            if (ModelState.IsValid)
            {
                // Cria a data e hora de início e término antes da consulta
                var startDateTime = new DateTime(model.RoomRequest.Date.Year, model.RoomRequest.Date.Month, model.RoomRequest.Date.Day, model.RoomRequest.StartTime.Hours, model.RoomRequest.StartTime.Minutes, 0);
                var endDateTime = new DateTime(model.RoomRequest.Date.Year, model.RoomRequest.Date.Month, model.RoomRequest.Date.Day, model.RoomRequest.EndTime.Hours, model.RoomRequest.EndTime.Minutes, 0);

                // Valida se a hora de término é posterior à de início
                if (endDateTime <= startDateTime)
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
                        var roomRequest = new RoomRequest
                        {
                            RoomId = model.RoomRequest.RoomId,
                            Date = model.RoomRequest.Date,
                            StartTime = startDateTime.TimeOfDay, // Guardar apenas TimeSpan
                            EndTime = endDateTime.TimeOfDay, // Guardar apenas TimeSpan
                            UserId = userId // Identificador do usuário
                        };

                        // Carrega todas as requisições de sala para memória
                        var existingRequests = _context.RoomRequests
                            .Where(rr => rr.RoomId == roomRequest.RoomId)
                            .ToList(); // Traz todos os pedidos da sala para a memória

                        // Verifica conflitos de horário na memória
                        var conflict = existingRequests.Any(rr =>
                            (startDateTime >= rr.Date.Date + rr.StartTime && startDateTime < rr.Date.Date + rr.EndTime) ||
                            (endDateTime > rr.Date.Date + rr.StartTime && endDateTime <= rr.Date.Date + rr.EndTime) ||
                            (startDateTime <= rr.Date.Date + rr.StartTime && endDateTime >= rr.Date.Date + rr.EndTime));

                        // Se houver conflito, adiciona erro ao ModelState
                        if (conflict)
                        {
                            ModelState.AddModelError(string.Empty, "O horário solicitado conflita com uma reserva existente.");
                        }
                        else
                        {
                            // Adiciona a requisição ao contexto e salva
                            _context.RoomRequests.Add(roomRequest);
                            _context.SaveChanges();

                            // Redireciona com base na role
                            return User.IsInRole("Admin") ? RedirectToAction("RoomAdmin") : RedirectToAction("RoomRequest");
                        }
                    }
                }
            }

            // Se houver erros, repopula o modelo e retorna à view correta
            var rooms = _context.Rooms.ToList();
            model.RoomRequest.Rooms = rooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList();

 
            // Redefine StartTime e EndTime para nulo ou um valor que acione a opção "Selecione..."
            model.RoomRequest.StartTime = TimeSpan.Zero;
            model.RoomRequest.EndTime = TimeSpan.Zero;


            return View("RoomAdmin", model);
        }

        [Authorize]
        public IActionResult MyRoomRequests()
        {
            // Obtém o UserId do usuário logado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verifica se o usuário é administrador
            var isAdmin = User.IsInRole("Admin");

            // Define a data atual
            var today = DateTime.Now.Date;

            // Busca os requisitos com base no papel do usuário
            List<RoomRequest> futureRequests;
            List<RoomRequest> pastRequests;

            if (isAdmin)
            {
                // Se for administrador, busca todos os futuros requisitos
                futureRequests = _context.RoomRequests
                    .Where(r => r.Date >= today) // define a data
                    .Include(r => r.Room) // Inclui dados das salas associadas
                    .Include(r => r.User) // Inclui os dados do usuário que fez o requisito
                    .OrderBy (r => r.Date) // ordena por data
                    .ThenBy (r => r.StartTime) // ordena por hora de inicio
                    .ToList();

                // busca os pedidos anterioes
                pastRequests = _context.RoomRequests
                     .Where(d => d.Date < today) 
                     .Include(d => d.Room)
                     .Include(d => d.User)
                     .OrderByDescending (d => d.Date) // ordena por ordem decrecente
                     .ThenBy(d => d.StartTime) // ordena por hora de início decrescente
                     .ToList();

            }
            else
            {
                // Se não for administrador, busca apenas os requisitos do usuário logado
                futureRequests = _context.RoomRequests
                    .Where(r => r.UserId == userId && r.Date >= today)
                    .Include(r => r.Room) // Inclui dados das salas associadas
                    .OrderBy(r => r.Date)
                    .ThenBy(r => r.StartTime)
                    .ToList();

                pastRequests = _context.RoomRequests
                   .Where(r => r.UserId == userId && r.Date < today) 
                   .Include(r => r.Room) // Inclui dados das salas associadas
                   .OrderByDescending(r => r.Date)
                   .ThenBy(r => r.StartTime)
                   .ToList();

            }

            // Retorna as listas para a view, por meio de um novo ViewModel
            var model = new RoomRequestTabels
            {
                FutureRequests = futureRequests,
                PastRequests = pastRequests
            };

            return View(model);
        }

        [Authorize]
        public IActionResult RoomAvailability()
        {
            var model = new RoomAvailabilityViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public IActionResult CheckAvailability(RoomAvailabilityViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Cria a data e hora de início e término antes da consulta
                var startDateTime = new DateTime(model.Date.Year, model.Date.Month, model.Date.Day, model.StartTime.Hours, model.StartTime.Minutes, 0);
                var endDateTime = new DateTime(model.Date.Year, model.Date.Month, model.Date.Day, model.EndTime.Hours, model.EndTime.Minutes, 0);

                // Verifica se a hora de término é posterior à hora de início
                if (endDateTime <= startDateTime)
                {
                    ModelState.AddModelError(string.Empty, "A hora de término deve ser posterior à hora de início.");
                    return View("RoomAvailability", model);
                }

                // Verificar se há salas já reservadas neste horário
                var bookedRooms = _context.RoomRequests
                    .Where(r => r.Date == model.Date.Date &&
                                r.StartTime < endDateTime.TimeOfDay &&
                                r.EndTime > startDateTime.TimeOfDay)
                    .Select(r => r.RoomId)
                    .ToList();

                // Listar salas disponíveis
                model.AvailableRooms = _context.Rooms
                    .Where(r => !bookedRooms.Contains(r.Id))
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.Name,
                    })
                    .ToList();

                // Define a flag indicando que uma pesquisa foi realizada
                ViewData["HasSearched"] = true;

                // Se não houver salas disponíveis, exibe a mensagem
                if (!model.AvailableRooms.Any())
                {
                    ModelState.AddModelError(string.Empty, "Não há salas disponíveis para o horário selecionado.");
                }

                return View("RoomAvailability", model);
            }
            // Redefine StartTime e EndTime para nulo ou um valor que acione a opção "Selecione..."
            model.StartTime = TimeSpan.Zero; 
            model.EndTime = TimeSpan.Zero; 

            return View("RoomAvailability", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
