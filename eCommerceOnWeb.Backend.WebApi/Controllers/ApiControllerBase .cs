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
    }
}
