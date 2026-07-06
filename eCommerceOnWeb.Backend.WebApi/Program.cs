using eCommerceOnWeb.Backend.Infrastructure.Data; // Подключаем слой инфраструктуры
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 1. НАСТРОЙКА И РЕГИСТРАЦИЯ DB_CONTEXT С ПРОВАЙДЕРОМ POSTGRESQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"), // Берем строку из appsettings.json
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("eCommerceOnWeb.Backend.WebApi"))); // Указываем, куда складывать миграции

// 2. Стандартные сервисы API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// Настройки Middleware (Swagger, Routing, Endpoints) остаются ниже без изменений
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();