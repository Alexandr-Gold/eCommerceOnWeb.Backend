using eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct;
using eCommerceOnWeb.Backend.Domain.Common.Interfaces;
using eCommerceOnWeb.Backend.Persistence.Data; // Подключаем слой инфраструктуры
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 1. НАСТРОЙКА И РЕГИСТРАЦИЯ DB_CONTEXT С ПРОВАЙДЕРОМ POSTGRESQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"), // Берем строку из appsettings.json
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("eCommerceOnWeb.Backend.Persistence"))); // Указываем, куда складывать миграции

// 2. Стандартные сервисы API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// 2. Регистрация MediatR для слоя Application (замените CreateProductCommand на любой класс из Application)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

// Регистрация репозитория для операций Записи и Чтения
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

// Регистрация репозитория ТОЛЬКО для Чтения
builder.Services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

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