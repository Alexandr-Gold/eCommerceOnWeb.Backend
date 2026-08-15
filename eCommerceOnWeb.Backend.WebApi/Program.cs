using eCommerceOnWeb.Backend.Application;
using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using eCommerceOnWeb.Backend.Domain.Common.Interfaces;
using eCommerceOnWeb.Backend.Persistence;
using eCommerceOnWeb.Backend.Persistence.Contexts; // Подключаем слой инфраструктуры
using eCommerceOnWeb.Backend.Persistence.Services;
using Microsoft.EntityFrameworkCore;
;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 2. Стандартные сервисы API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();

builder.Services.AddPersistence(
    builder.Configuration);

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

using (IServiceScope scope = app.Services.CreateScope())
{
    ECommerceDbContext context =
        scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

    await context.Database.MigrateAsync();

    await ECommerceDbContextSeed.SeedAsync(context);
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();