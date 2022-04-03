using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Entities;
using API.Entities.OrderAggregate;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class OrdersController : BaseApiController {
    private readonly StoreContext _context;

    public OrdersController(StoreContext context) {
        _context = context;
    }

    [HttpGet, Authorize]
    public async Task<ActionResult<List<OrderDto>>> GetOrders() {
        var orders = await _context.Orders.ProjectOrderToOrderDto()
            .Where(x => x.BuyerId == User.Identity.Name)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpGet("{id:int}", Name = "GetOrder"), Authorize]
    public async Task<ActionResult<OrderDto>> GetOrder(int id) {
        var order = await _context.Orders.ProjectOrderToOrderDto()
            .Where(x => x.BuyerId == User.Identity.Name && x.Id == id)
            .FirstOrDefaultAsync();
        return Ok(order);
    }

    [HttpPost, Authorize]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderDto orderDto) {
        var basket = await _context.Baskets.RetrieveBasketWithItems(User.Identity?.Name)
            .FirstOrDefaultAsync();

        if (basket == null) return BadRequest(new ProblemDetails { Title = "Could not locate Basket" });

        var items = new List<OrderItem>();
        foreach (var item in basket.Items) {
            var productItem = await _context.Products.FindAsync(item.ProductId);
            if (productItem == null) continue;
            var itemOrdered = new ItemOrdered {
                ProductId = productItem.Id,
                Name = productItem.Name,
                ImageUrl = productItem.ImageUrl
            };

            var orderItem = new OrderItem {
                ItemOrdered = itemOrdered,
                Price = productItem.Price,
                Quantity = item.Quantity
            };
            items.Add(orderItem);
            productItem.QtyInStock -= item.Quantity;
        }

        var subtotal = items.Sum(item => item.Price * item.Quantity);
        var deliveryFee = subtotal > 10000 ? 0 : 500;

        var order = new Order {
            OrderItems = items,
            BuyerId = User.Identity?.Name,
            ShippingAddress = orderDto.ShippingAddress,
            Subtotal = subtotal,
            DeliveryFee = deliveryFee,
            PaymentIntentId = basket.PaymentIntentId
        };

        _context.Orders.Add(order);
        _context.Baskets.Remove(basket);

        if (orderDto.SaveAddress) {
            var user = await _context.Users
                .Include(u => u.Address)
                .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            if (user != null) {
                var address = new UserAddress {
                    FullName = orderDto.ShippingAddress.FullName,
                    Address1 = orderDto.ShippingAddress.Address1,
                    Address2 = orderDto.ShippingAddress.Address2,
                    City = orderDto.ShippingAddress.City,
                    Country = orderDto.ShippingAddress.Country,
                    ZipCode = orderDto.ShippingAddress.ZipCode,
                    State = orderDto.ShippingAddress.State,
                };
                user.Address = address;
            }
        }

        var result = await _context.SaveChangesAsync() > 0;
        return result
            ? CreatedAtRoute("GetOrder", new { id = order.Id }, order.Id)
            : BadRequest("Unable to create order");
    }
}