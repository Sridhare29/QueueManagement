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
                return CreatedAtAction(nameof(GetStatus), new { tokenNo = result.TokenNo }, result);
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

        // POST api/queue/call-next/{tokenNo}
        [HttpPost("call-next/{tokenNo}")]
        public async Task<IActionResult> CallNext(string tokenNo, [FromBody] CallNextRequest request)
        {
            try
            {
                var result = await _service.CallNext(tokenNo, request.CounterId);
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

        // PUT api/queue/complete/{tokenNo}
        [HttpPut("complete/{tokenNo}")]
        public async Task<IActionResult> Complete(string tokenNo)
        {
            try
            {
            var result = await _service.CompleteToken(tokenNo);
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

        // GET api/queue/status/{tokenNo}
        [HttpGet("status/{tokenNo}")]
        public async Task<IActionResult> GetStatus(string tokenNo)
        {
            try
            {
            var result = await _service.GetTokenStatus(tokenNo);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
        }
    }
}