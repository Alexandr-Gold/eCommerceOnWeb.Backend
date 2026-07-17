using eCommerceOnWeb.Backend.Application.Features.Products.Commands.CreateProduct;
using eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceOnWeb.Backend.WebApi.Controllers
{
    public class ProductsController : ApiControllerBase
    {
        /// <summary>
        /// Создать новую карточку товара электроники.
        /// </summary>
        /// <param name="command">Данные для создания товара</param>
        /// <returns>Guid созданного продукта</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            Guid productId = await Mediator.Send(command);

            // Возвращаем статус 201 Created и ссылку на ресурс (маршрут GetById мы напишем позже)
            return CreatedAtAction(nameof(Create), new { id = productId }, productId);
        }

        /// <summary>
        /// Получить список товаров с фильтрацией и пагинацией (Каталог).
        /// </summary>
        /// <param name="query">Параметры фильтрации и пагинации</param>
        /// <returns>Постраничный список товаров</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<ProductDto>))]
        public async Task<ActionResult<PaginatedList<ProductDto>>> GetWithPagination([FromQuery] GetProductsWithPaginationQuery query)
        {
            PaginatedList<ProductDto> result = await Mediator.Send(query);
            return Ok(result);
        }
    }
}
