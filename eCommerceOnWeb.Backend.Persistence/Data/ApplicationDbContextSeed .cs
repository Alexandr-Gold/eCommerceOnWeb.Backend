using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerceOnWeb.Backend.Persistence.Data
{
    public class ApplicationDbContextSeed
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicationDbContextSeed> _logger;

        public ApplicationDbContextSeed(ApplicationDbContext context, ILogger<ApplicationDbContextSeed> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                if (_context.Database.IsRelational())
                {
                    _logger.LogInformation("Проверка и применение миграций для PostgreSQL...");
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Миграции успешно применены.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при автоматическом применении миграций.");
                throw;
            }
        }
    }


}


