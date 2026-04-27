using Microsoft.AspNetCore.Mvc;
using SdiApiGateway.Agents;

namespace SdiApiGateway.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    private readonly InterviewAgent _interviewAgent;

    public HealthController(InterviewAgent interviewAgent)
    {
        _interviewAgent = interviewAgent;
    }

    [HttpGet("health")]
    [HttpGet("api/v1/health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "UP",
            service = "SDI API Gateway (.NET 10)",
            timestamp = DateTime.UtcNow.ToString("o")
        });
    }

    /// <summary>A2A Agent discovery endpoint.</summary>
    [HttpGet(".well-known/agent-cards")]
    public IActionResult AgentCards()
    {
        return Ok(_interviewAgent.DiscoverAgents());
    }
}
