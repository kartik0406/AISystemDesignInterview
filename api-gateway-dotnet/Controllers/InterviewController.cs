using Microsoft.AspNetCore.Mvc;
using SdiApiGateway.Models.DTOs;
using SdiApiGateway.Services;

namespace SdiApiGateway.Controllers;

[ApiController]
[Route("api/v1/interview")]
public class InterviewController : ControllerBase
{
    private readonly InterviewService _interviewService;
    private readonly ILogger<InterviewController> _logger;

    public InterviewController(InterviewService interviewService, ILogger<InterviewController> logger)
    {
        _interviewService = interviewService;
        _logger = logger;
    }

    /// <summary>Start a new interview session.</summary>
    [HttpPost("start")]
    public async Task<IActionResult> StartInterview([FromBody] StartInterviewRequest request)
    {
        _logger.LogInformation("POST /interview/start — topic={Topic}, company={Company}", request.Topic, request.CompanyMode);
        var response = await _interviewService.StartInterviewAsync(request);
        return Ok(response);
    }

    /// <summary>Submit an answer for the current round.</summary>
    [HttpPost("answer")]
    public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequest request)
    {
        _logger.LogInformation("POST /interview/answer — session={SessionId}", request.SessionId);
        var response = await _interviewService.SubmitAnswerAsync(request);
        return Ok(response);
    }

    /// <summary>Get current session state.</summary>
    [HttpGet("session/{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id)
    {
        _logger.LogInformation("GET /interview/session/{SessionId}", id);
        var response = await _interviewService.GetSessionAsync(id);
        return Ok(response);
    }

    /// <summary>Get final evaluation report.</summary>
    [HttpGet("result/{id:guid}")]
    public async Task<IActionResult> GetResult(Guid id)
    {
        _logger.LogInformation("GET /interview/result/{SessionId}", id);
        var report = await _interviewService.GenerateReportAsync(id);
        return Ok(report);
    }

    /// <summary>Request a hint for the current question.</summary>
    [HttpPost("hint")]
    public async Task<IActionResult> RequestHint([FromBody] HintRequest request)
    {
        _logger.LogInformation("POST /interview/hint — session={SessionId}, level={Level}", request.SessionId, request.HintLevel);
        var hint = await _interviewService.RequestHintAsync(request);
        return Ok(new { hint });
    }

    /// <summary>Get available interview topics.</summary>
    [HttpGet("topics")]
    public IActionResult GetTopics()
    {
        _logger.LogInformation("GET /interview/topics");
        return Ok(_interviewService.GetTopics());
    }
}
