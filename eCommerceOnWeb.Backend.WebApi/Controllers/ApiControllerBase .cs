using eCommerceOnWeb.Backend.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceOnWeb.Backend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private IMediator? _mediator;

        // Автоматически лениво подтягивает MediatR из DI, чтобы не внедрять его через конструктор в каждом контроллере
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            // Если код ошибки заканчивается на .NotFound, отдаем 404 Status Code
            if (result.Error.Code.EndsWith(".NotFound"))
            {
                return NotFound(new { error = result.Error });
            }

            // Все остальные контролируемые бизнес-ошибки отдаем как 400 Bad Request
            return BadRequest(new { error = result.Error });
        }
    }
}
