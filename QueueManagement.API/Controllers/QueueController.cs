using Microsoft.AspNetCore.Mvc;
using QueueManagement.Application.DTOs;
using QueueManagement.Application.Interfaces;

namespace QueueManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueService _service;

        public QueueController(IQueueService service)
        {
            _service = service;
        }

        // POST api/queue/generate
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateTokenRequest request)
        {
            try
            {
                var result = await _service.GenerateToken(request);
                return CreatedAtAction(nameof(GetStatus), new { tokenId = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse(ex.Message));
            }
        }

        // POST api/queue/call-next/{counterId}
        [HttpPost("call-next/{counterId:int}")]
        public async Task<IActionResult> CallNext(int counterId)
        {
            try
            {
                var result = await _service.CallNext(counterId);
                if (result == null)
                    return NoContent(); // no one waiting

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse(ex.Message));
            }
        }

        // PUT api/queue/complete/{tokenId}
        [HttpPut("complete/{tokenId:int}")]
        public async Task<IActionResult> Complete(int tokenId)
        {
            try
            {
                var result = await _service.CompleteToken(tokenId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse(ex.Message));
            }
        }

        // GET api/queue/waiting
        [HttpGet("waiting")]
        public async Task<IActionResult> Waiting()
        {
            return Ok(await _service.GetWaitingQueue());
        }

        // GET api/queue/status/{tokenId}
        [HttpGet("status/{tokenId:int}")]
        public async Task<IActionResult> GetStatus(int tokenId)
        {
            try
            {
                var result = await _service.GetTokenStatus(tokenId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
        }
    }
}