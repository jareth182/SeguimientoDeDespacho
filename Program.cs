using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services; // <-- 1. AÑADIR ESTE USING
using Microsoft.EntityFrameworkCore;
using SeguimientoDeDespacho.Data;
using SeguimientoDeDespacho.Services; // <-- 2. AÑADIR ESTE USING

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Modificar esta línea
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // <-- 3. AÑADIR ESTO (Importante para generar el token)

builder.Services.AddControllersWithViews();

// --- INICIO DE MODIFICACIÓN HU02 ---
// 4. Registrar nuestro servicio falso de email
builder.Services.AddTransient<IEmailSender, DummyEmailSender>();
// --- FIN DE MODIFICACIÓN HU02 ---


var app = builder.Build();

// --- INICIO DE SEEDING (SEMBRADO) ---
// (Este bloque ya lo tenías, déjalo como está)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        await SeedRoles(roleManager);
        await SeedUsers(userManager);

        logger.LogInformation("Base de datos sembrada exitosamente (Roles y Usuarios).");
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

app.UseAuthentication(); // <-- Ya lo tenías
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
// (Estas funciones ya las tenías, déjalas como están)
async Task SeedRoles(RoleManager<IdentityRole> roleManager)
{
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
            EmailConfirmed = true 
        };
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
            EmailConfirmed = true 
        };
        var result = await userManager.CreateAsync(clienteUser, "Cliente123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(clienteUser, "Cliente");
        }
    }
}