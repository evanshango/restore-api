using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.RequestHelpers;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class ProductsController : BaseApiController {
    private readonly StoreContext _context;
    private readonly IMapper _mapper;
    private readonly ImageService _imageService;

    public ProductsController(StoreContext context, IMapper mapper, ImageService imageService) {
        _context = context;
        _mapper = mapper;
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<Product>>> GetProducts([FromQuery] ProductParams productParams) {
        var query = _context.Products.Sort(productParams.OrderBy).Search(productParams.SearchTerm)
            .Filter(productParams.Brands, productParams.Types)
            .AsQueryable();
        var products = await PagedList<Product>.ToPagedList(query, productParams.Page, productParams.PageSize);
        Response.AddPaginationHeader(products.MetaData);
        return Ok(products);
    }

    [HttpGet("{id:int}", Name = "GetProduct")]
    public async Task<ActionResult<Product>> GetProduct(int id) {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpGet("filters")]
    public async Task<IActionResult> GetFilters() {
        var brands = await _context.Products.Select(p => p.Brand).Distinct().ToListAsync();
        var types = await _context.Products.Select(p => p.Type).Distinct().ToListAsync();

        return Ok(new { brands, types });
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<ActionResult<Product>> CreateProduct([FromForm] CreateProductDto productDto) {
        var product = _mapper.Map<Product>(productDto);

        if (productDto.Image != null) {
            var imageResult = await _imageService.AddImageAsync(productDto.Image);
            if (imageResult.Error != null) return BadRequest(new ProblemDetails { Title = imageResult.Error.Message });

            product.ImageUrl = imageResult.SecureUrl.ToString();
            product.PublicId = imageResult.PublicId;
        }

        _context.Products.Add(product);
        var result = await _context.SaveChangesAsync() > 0;

        if (result) return CreatedAtRoute("GetProduct", new { product.Id }, product);

        return BadRequest(new ProblemDetails { Title = "Problem creating new Product" });
    }

    [HttpPut, Authorize(Roles = "Admin")]
    public async Task<ActionResult<Product>> UpdateProduct([FromForm] UpdateProductDto productDto) {
        var product = await _context.Products.FindAsync(productDto.Id);
        if (product == null) return NotFound();

        _mapper.Map(productDto, product);

        if (productDto.Image != null) {
            var imageResult = await _imageService.AddImageAsync(productDto.Image);
            if (imageResult.Error != null) return BadRequest(new ProblemDetails { Title = imageResult.Error.Message });

            if (!string.IsNullOrEmpty(product.PublicId)) await _imageService.DeleteImageAsync(product.PublicId);

            product.ImageUrl = imageResult.SecureUrl.ToString();
            product.PublicId = imageResult.PublicId;
        }

        var result = await _context.SaveChangesAsync() > 0;

        if (result) return Ok(await _context.Products.FindAsync(product.Id));

        return BadRequest(new ProblemDetails { Title = "Problem updating product" });
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id) {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        
        if (!string.IsNullOrEmpty(product.PublicId)) await _imageService.DeleteImageAsync(product.PublicId);

        _context.Products.Remove(product);

        var result = await _context.SaveChangesAsync() > 0;

        if (result) return Ok();

        return BadRequest(new ProblemDetails { Title = "Problem deleting product" });
    }
}