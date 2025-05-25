using Microsoft.EntityFrameworkCore;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load(); // This will load .env from the current directory (project root)

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication();

// Configure settings from appsettings.json and environment variables
builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
builder.Configuration.AddEnvironmentVariables();

// Add AutoMapper for object mapping
builder.Services.AddAutoMapper(typeof(MappingProfile));

var BacKOfficeAllowCors = "_backOfficeAllowCORS";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: BacKOfficeAllowCors,
                      policy =>
                      {
                          // For more permissive settings (less secure, use with caution):
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

//builder.Services.ConfigureHttpJsonOptions(options =>
//{
//    options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
//});

// Register services
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<EventsService>();
builder.Services.AddScoped<BarcodeService>();
builder.Services.AddSingleton<Globals>();

// Configure the database context
builder.Services.AddDbContextFactory<ApplicationContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
        throw new Exception("Not a valid connection string or database not reachable");
    options.UseMySQL(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(BacKOfficeAllowCors);

// Install database and generate first user
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationContext>>();
    var db = factory.CreateDbContext();
    await db.Database.MigrateAsync();

    // Create admin user if it doesn't exist
    var userService = scope.ServiceProvider.GetRequiredService<UsersService>();
    var admin = await userService.GetUserAsync(1);
    if (admin == null)
    {
        await userService.CreateUserAsync(
            new UserCreateDto
            {
                Username = "admin",
                Name = "Admin",
                Type = UserType.Admin,
                Badge = 0,
                Password = "admin"
            }, true);
    }

    // Generate authentication filters
    var globals = scope.ServiceProvider.GetRequiredService<Globals>();
    var hostEnv = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    globals.AdminAuth = new BasicAuthFilter(userService, [UserType.Admin], hostEnv);
    globals.ProfessorAuth = new BasicAuthFilter(userService, [UserType.Professor], hostEnv);
    globals.StudentAuth = new BasicAuthFilter(userService, [UserType.Student], hostEnv);
    globals.AllAuth = new BasicAuthFilter(userService, [], hostEnv);
}

// Add controllers
app.AddUsersController();
app.AddEventsController();
app.AddBarcodeController();

await app.RunAsync();