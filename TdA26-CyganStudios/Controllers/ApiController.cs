using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TdA26_CyganStudios.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    [HttpGet]
    public ApiResponse Get()
    {
        return new ApiResponse("Student Cyber Games");
    }

    public sealed record ApiResponse(string Organization);
}
