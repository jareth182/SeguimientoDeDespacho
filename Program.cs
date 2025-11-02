using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeguimientoDeDespacho.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Habilita Identity, Roles y deshabilita la confirmación de email
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() 
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // 1. Llamar a la función para crear Roles
        await SeedRoles(roleManager);
        
        // 2. Llamar a la función para crear Usuarios
        await SeedUsers(userManager);

        logger.LogInformation("Base de datos sembrada exitosamente (Roles y Usuarios).");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Un error ocurrió al sembrar la base de datos.");
    }
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

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();


// --- FUNCIONES DE SEEDING ---

async Task SeedRoles(RoleManager<IdentityRole> roleManager)
{
    // Función para crear los roles "Admin" y "Cliente"
    string[] roleNames = { "Admin", "Cliente" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

async Task SeedUsers(UserManager<IdentityUser> userManager)
{
    // 1. Crear usuario Administrador
    string adminEmail = "admin@correo.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true // Se salta la confirmación de email
        };
        // Contraseña: "Admin123!"
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
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
            EmailConfirmed = true // Se salta la confirmación de email
        };
        // Contraseña: "Cliente123!"
        var result = await userManager.CreateAsync(clienteUser, "Cliente123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(clienteUser, "Cliente");
        }
    }
}

