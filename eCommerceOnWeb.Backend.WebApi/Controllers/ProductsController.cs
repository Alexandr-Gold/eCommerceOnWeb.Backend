using eCommerceOnWeb.Backend.Application.Features.Products.Queries.CreateProduct;
using eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProductDetails;
using eCommerceOnWeb.Backend.Application.Features.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceOnWeb.Backend.WebApi.Controllers
{
    public class ProductsController : ApiControllerBase
    {
        /// <summary>
        /// Получить подробную информацию о товаре по его Guid (включая галерею картинок из MinIO)
        /// </summary>
        /// <param name="id">Идентификатор товара</param>
        /// <param name="cancellationToken">Токен отмены запроса</param>
        /// <returns>JSON с данными товара или 404 Not Found</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDetailsDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            // 1. Создаем запрос
            GetProductDetailsQuery query = new GetProductDetailsQuery(id);

            // 2. Отправляем в MediatR. Теперь тип 'result' — это Result<ProductDetailsDto>
            Domain.Common.Result<ProductDetailsDto> result = await Mediator.Send(query, cancellationToken);

            // 3. Передаем контейнер в базовый метод. 
            // Он сам проверит .IsSuccess, достанет .Value или сгенерирует 404/400 ошибку.
            return HandleResult(result);
        }

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
