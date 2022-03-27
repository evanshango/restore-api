using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Entities;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController {
    private readonly UserManager<User> _userManager;
    private readonly TokenService _tokenService;
    private readonly StoreContext _context;

    public AccountController(UserManager<User> userManager, TokenService tokenService, StoreContext context) {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("signin")]
    public async Task<ActionResult<UserDto>> Signin([FromBody] SigninDto signinDto) {
        var user = await _userManager.FindByNameAsync(signinDto.Username);

        if (user == null || !await _userManager.CheckPasswordAsync(user, signinDto.Password)) return Unauthorized();

        var userBasket = await RetrieveBasket(signinDto.Username);
        var anonymousBasket = await RetrieveBasket(Request.Cookies["buyerId"]);

        if (anonymousBasket == null)
            return Ok(new UserDto {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Basket = userBasket?.MapBasketToDto()
            });
        if (userBasket != null) _context.Baskets.Remove(userBasket);
        anonymousBasket.BuyerId = user.UserName;
        Response.Cookies.Delete("buyerId");
        await _context.SaveChangesAsync();

        return Ok(new UserDto {
            Email = user.Email,
            Token = await _tokenService.GenerateToken(user),
            Basket = anonymousBasket.MapBasketToDto()
        });
    }

    [HttpPost("signup")]
    public async Task<ActionResult> Signup([FromBody] SignupDto signupDto) {
        var user = new User {
            UserName = signupDto.Username,
            Email = signupDto.Email
        };
        var result = await _userManager.CreateAsync(user, signupDto.Password);

        if (!result.Succeeded) {
            foreach (var error in result.Errors) {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem();
        }

        await _userManager.AddToRoleAsync(user, "Member");
        return StatusCode(201);
    }

    [HttpGet("current/user"), Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser() {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name);
        var userBasket = await RetrieveBasket(User.Identity?.Name);
        return Ok(new UserDto {
            Email = user.Email,
            Token = await _tokenService.GenerateToken(user),
            Basket = userBasket?.MapBasketToDto()
        });
    }

    private async Task<Basket> RetrieveBasket(string buyerId) {
        if (!string.IsNullOrEmpty(buyerId))
            return await _context.Baskets
                .Include(i => i.Items)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(x => x.BuyerId == buyerId);

        Response.Cookies.Delete("buyerId");
        return null;
    }
}