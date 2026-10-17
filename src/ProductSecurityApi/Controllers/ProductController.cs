using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductSecurityApi.Attributes;
using ProductSecurityApi.Models;

namespace ProductSecurityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Ноутбук ASUS ROG Strix", Price = 650000 },
        new Product { Id = 2, Name = "Беспроводная мышь Logitech G Pro", Price = 45000 },
        new Product { Id = 3, Name = "Механическая клавиатура Keychron K8", Price = 58000 },
        new Product { Id = 4, Name = "Монитор LG UltraGear 27''", Price = 180000 }
    };

    /// <summary>
    /// Задание 1: Базовый эндпоинт получения списка товаров
    /// </summary>
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_products);
    }

    /// <summary>
    /// Получение одного товара по идентификатору
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound(new { message = $"Товар с ID {id} не найден." });
        }
        return Ok(product);
    }

    /// <summary>
    /// Задание 2: Защита эндпоинта с помощью заголовка X-Api-Key: SecretKey123
    /// </summary>
    [HttpGet("api-key-protected")]
    [ApiKey]
    public IActionResult GetWithApiKey()
    {
        return Ok(new
        {
            message = "Доступ успешно предоставлен по валидному API-ключу (X-Api-Key: SecretKey123)!",
            totalProducts = _products.Count,
            data = _products
        });
    }

    /// <summary>
    /// Задания 5, 6, 7: Защита эндпоинта с помощью атрибута [Authorize] (JWT Bearer Token)
    /// </summary>
    [HttpGet("secure")]
    [Authorize]
    public IActionResult GetSecure()
    {
        var userName = User.Identity?.Name ?? "Неизвестный пользователь";
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Без роли";

        return Ok(new
        {
            message = "Доступ успешно предоставлен по валидному JWT Bearer токену!",
            authorizedUser = userName,
            role = userRole,
            data = _products
        });
    }

    /// <summary>
    /// Добавление товара (защищено JWT)
    /// </summary>
    [HttpPost]
    [Authorize]
    public IActionResult Create([FromBody] Product product)
    {
        if (product == null)
        {
            return BadRequest(new { message = "Некорректные данные товара." });
        }

        var newId = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
        product.Id = newId;
        _products.Add(product);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}
