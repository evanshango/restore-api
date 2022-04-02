using System.Linq;
using API.Dtos;
using API.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class OrderExtensions {
    public static IQueryable<OrderDto> ProjectOrderToOrderDto(this IQueryable<Order> query) {
        return query.Select(order => new OrderDto {
            Id = order.Id,
            BuyerId = order.BuyerId,
            OrderDate = order.OrderDate,
            ShippingAddress = order.ShippingAddress,
            DeliveryFee = order.DeliveryFee,
            Subtotal = order.Subtotal,
            OrderStatus = order.OrderStatus.ToString(),
            Total = order.GetTotal(),
            OrderItems = order.OrderItems.Select(item => new OrderItemDto {
                ProductId = item.ItemOrdered.ProductId,
                Name = item.ItemOrdered.Name,
                ImageUrl = item.ItemOrdered.ImageUrl,
                Price = item.Price,
                Quantity = item.Quantity
            }).ToList()
        }).AsNoTracking();
    }
}