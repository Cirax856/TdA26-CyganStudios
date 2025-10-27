using Microsoft.AspNetCore.Mvc;

namespace TdA26_CyganStudios.Controllers;

[ApiController]
[Route("api")]
public sealed class DummyController : ControllerBase
{
    [HttpGet]
    public OrganizationResponse Get()
    {
        return new OrganizationResponse("Student Cyber Games");
    }

    public sealed record OrganizationResponse(string Organization);
}
