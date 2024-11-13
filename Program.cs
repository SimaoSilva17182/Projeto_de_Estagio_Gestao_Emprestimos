using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectEstágio.Data;
using ProjectEstagio.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity with roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Adiciona o serviço de limpeza 
builder.Services.AddHostedService<RoomRequestCleanupService>();
builder.Services.AddHostedService<VeiculeRequestCleanupService>();
builder.Services.AddHostedService<EquipmentRequestCleanupService>();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Método para criar as Roles e atribuí-las aos usuários
using (var scope = app.Services.CreateScope()) // Cria um escopo explícito
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>(); // Logger para depuração
    logger.LogInformation("Starting role creation and user assignment...");
    await CreateRolesAndAssignToUsers(services, logger);
    logger.LogInformation("Role creation and user assignment completed.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

// Método para criar as Roles e atribuí-las aos usuários
async Task CreateRolesAndAssignToUsers(IServiceProvider serviceProvider, ILogger logger)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Criação das roles
    string[] roleNames = { "Admin", "GestorSalas", "GestorVeiculos", "GestorEquipamentos" };

    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                logger.LogInformation($"Role '{roleName}' created successfully.");
            }
            else
            {
                logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            logger.LogInformation($"Role '{roleName}' already exists.");
        }
    }

    // Atribuição de roles aos usuários específicos
    await AssignRoleToUser(userManager, logger, "Admin@gmail.com", "Admin");
    await AssignRoleToUser(userManager, logger, "GestorSalas@gmail.com", "GestorSalas");
    await AssignRoleToUser(userManager, logger, "GestorVeiculos@gmail.com", "GestorVeiculos");
    await AssignRoleToUser(userManager, logger, "GestorEquipamentos@gmail.com", "GestorEquipamentos");
}

async Task AssignRoleToUser(UserManager<IdentityUser> userManager, ILogger logger, string email, string roleName)
{
    var user = await userManager.FindByEmailAsync(email);
    if (user != null)
    {
        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var result = await userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                logger.LogInformation($"Role '{roleName}' assigned to user '{email}'.");
            }
            else
            {
                logger.LogError($"Failed to assign role '{roleName}' to user '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            logger.LogInformation($"User '{email}' already has the role '{roleName}'.");
        }
    }
    else
    {
        logger.LogWarning($"User with email '{email}' not found.");
    }
}
