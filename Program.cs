using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services; 
using Microsoft.EntityFrameworkCore;
using SeguimientoDeDespacho.Data;
using SeguimientoDeDespacho.Services; 
using SeguimientoDeDespacho.Models; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Modificar esta línea
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Habilita Roles
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Para el reseteo de contraseña

builder.Services.AddControllersWithViews();

// Registrar nuestro servicio falso de email
builder.Services.AddTransient<IEmailSender, DummyEmailSender>();


var app = builder.Build();

// --- INICIO DE SEEDING (SEMBRADO) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>(); 

        // El orden es importante:
        await SeedRoles(roleManager, logger); // 1. Crear Roles
        await SeedUsers(userManager, logger); // 2. Crear Usuarios
        await SeedDespachos(dbContext, logger); // 3. Crear Despachos
        
        logger.LogInformation("Base de datos sembrada exitosamente (Roles, Usuarios y Despachos).");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Un error ocurrió al sembrar la base de datos.");
    }
}
// --- FIN DE SEEDING ---


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

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();


// --- FUNCIONES DE SEEDING ---

async Task SeedRoles(RoleManager<IdentityRole> roleManager, ILogger<Program> logger)
{
    // Función para crear los roles "Admin" y "Cliente"
    string[] roleNames = { "Admin", "Cliente" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded) {
                logger.LogInformation($"Rol '{roleName}' creado exitosamente.");
            } else {
                logger.LogWarning($"Error al crear el rol '{roleName}'.");
            }
        }
    }
}

async Task SeedUsers(UserManager<IdentityUser> userManager, ILogger<Program> logger)
{
    // 1. Crear usuario Administrador
    string adminEmail = "admin@correo.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true 
        };
        // Contraseña: "Admin123!"
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        
        if (result.Succeeded)
        {
            logger.LogInformation("Usuario 'Admin' creado.");
            // --- INICIO DE LA CORRECCIÓN ---
            // Nos aseguramos de que el rol "Admin" exista ANTES de asignarlo
            if (await userManager.IsInRoleAsync(adminUser, "Admin") == false)
            {
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Rol 'Admin' asignado al usuario 'Admin'.");
                }
                else
                {
                    logger.LogError("ERROR: No se pudo asignar el rol 'Admin' al usuario 'Admin'.");
                }
            }
            // --- FIN DE LA CORRECCIÓN ---
        }
    }

    // 2. Crear usuario Cliente
    string clienteEmail = "cliente@correo.com";
    if (await userManager.FindByEmailAsync(clienteEmail) == null)
    {
        var clienteUser = new IdentityUser
        {
            UserName = clienteEmail,
            Email = clienteEmail,
            EmailConfirmed = true 
        };
        // Contraseña: "Cliente123!"
        var result = await userManager.CreateAsync(clienteUser, "Cliente123!");
        if (result.Succeeded)
        {
            logger.LogInformation("Usuario 'Cliente' creado.");
            // --- INICIO DE LA CORRECCIÓN ---
            if (await userManager.IsInRoleAsync(clienteUser, "Cliente") == false)
            {
                var roleResult = await userManager.AddToRoleAsync(clienteUser, "Cliente");
                 if (roleResult.Succeeded)
                {
                    logger.LogInformation("Rol 'Cliente' asignado al usuario 'Cliente'.");
                }
                else
                {
                    logger.LogError("ERROR: No se pudo asignar el rol 'Cliente' al usuario 'Cliente'.");
                }
            }
            // --- FIN DE LA CORRECCIÓN ---
        }
    }
}

async Task SeedDespachos(ApplicationDbContext context, ILogger<Program> logger)
{
    // Función para crear Despachos de prueba si no existen
    if (!context.Despachos.Any())
    {
        context.Despachos.AddRange(
            new Despacho
            {
                NumeroGuia = "T-001",
                ClienteNombre = "Cliente A",
                Estado = EstadoDespacho.EnProceso,
                FechaCreacion = DateTime.Now.AddDays(-2)
            },
            new Despacho
            {
                NumeroGuia = "T-002",
                ClienteNombre = "Cliente B",
                Estado = EstadoDespacho.EnProceso,
                FechaCreacion = DateTime.Now.AddDays(-1)
            },
            new Despacho
            {
                NumeroGuia = "T-003",
                ClienteNombre = "Cliente C",
                Estado = EstadoDespacho.Culminado,
                FechaCreacion = DateTime.Now.AddDays(-3)
            }
        );
        await context.SaveChangesAsync();
        logger.LogInformation("Despachos de prueba creados.");
    }
}

