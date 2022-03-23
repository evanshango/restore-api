using Microsoft.AspNetCore.Mvc;

namespace API.Controllers; 

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BaseApiController: ControllerBase {
    
}