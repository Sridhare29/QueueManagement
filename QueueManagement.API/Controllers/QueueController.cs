using Microsoft.AspNetCore.Mvc;
using QueueManagement.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class QueueController : ControllerBase
{
    private readonly IQueueService _service;

    public QueueController(IQueueService service)
    {
        _service = service;
    }

    [HttpPost("generate/{userId}")]
    public async Task<IActionResult> Generate(int userId)
    {
        var result = await _service.GenerateToken(userId);

        return Ok(result);
    }

    [HttpPost("call-next/{counterId}")]
    public async Task<IActionResult> CallNext(int counterId)
    {
        var result = await _service.CallNext(counterId);

        return Ok(result);
    }

    [HttpPut("complete/{tokenId}")]
    public async Task<IActionResult> Complete(int tokenId)
    {
        await _service.CompleteToken(tokenId);

        return Ok();
    }

    [HttpGet("waiting")]
    public async Task<IActionResult> Waiting()
    {
        return Ok(await _service.GetWaitingQueue());
    }
}