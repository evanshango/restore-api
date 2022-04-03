using System.IO;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Entities.OrderAggregate;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace API.Controllers;

public class PaymentsController : BaseApiController {
    private readonly PaymentService _paymentService;
    private readonly StoreContext _context;
    private readonly IConfiguration _config;

    public PaymentsController(PaymentService paymentService, StoreContext context, IConfiguration config) {
        _paymentService = paymentService;
        _context = context;
        _config = config;
    }

    [HttpPost, Authorize]
    public async Task<ActionResult<BasketDto>> CreateOrUpdatePaymentIntent() {
        var basket = await _context.Baskets.RetrieveBasketWithItems(User.Identity?.Name).FirstOrDefaultAsync();

        if (basket == null) return NotFound();

        var intent = await _paymentService.CreateOrUpdatePaymentIntent(basket);

        if (intent == null) return BadRequest(new ProblemDetails { Title = "Error creating Payment Intent" });

        basket.PaymentIntentId ??= intent.Id;
        basket.ClientSecret ??= intent.ClientSecret;

        _context.Update(basket);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest(new ProblemDetails { Title = "Error updating basket with payment intent" });
        return Ok(basket.MapBasketToDto());
    }

    [HttpPost("stripe/webhook"), ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult> StripeWebhook() {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        var stripeEvent = EventUtility.ConstructEvent(
            json, Request.Headers["Stripe-Signature"], _config["StripeSettings:WhSecret"]
        );

        var charge = (Charge)stripeEvent.Data.Object;

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.PaymentIntentId == charge.PaymentIntentId);

        if (charge.Status == "succeeded") order!.OrderStatus = OrderStatus.PaymentReceived;

        await _context.SaveChangesAsync();

        return new EmptyResult();
    }
}