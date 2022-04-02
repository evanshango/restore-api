using Microsoft.EntityFrameworkCore;

namespace API.Entities.OrderAggregate; 

[Owned]
public class ItemOrdered {
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
}