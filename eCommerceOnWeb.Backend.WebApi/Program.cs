using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Application.Features.Products.Queries.CreateProduct;
using eCommerceOnWeb.Backend.Domain.Common.Interfaces;
using eCommerceOnWeb.Backend.Persistence.Contexts; // Подключаем слой инфраструктуры
using eCommerceOnWeb.Backend.Persistence.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 1. НАСТРОЙКА И РЕГИСТРАЦИЯ DB_CONTEXT С ПРОВАЙДЕРОМ POSTGRESQL
builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"), // Берем строку из appsettings.json
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("eCommerceOnWeb.Backend.Persistence"))); // Указываем, куда складывать миграции

// ОБЯЗАТЕЛЬНАЯ РЕГИСТРАЦИЯ ИНТЕРФЕЙСА ДЛЯ СЛОЯ APPLICATION
builder.Services.AddScoped<IECommerceDbContext>(provider =>
    provider.GetRequiredService<ECommerceDbContext>());

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

// Регистрация сервиса генерации путей к картинкам
builder.Services.AddScoped<IStorageService, StorageService>();

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