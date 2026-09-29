using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QueueManagement.Application.DTOs;
using QueueManagement.Application.Interfaces;

namespace QueueManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QueueController : ControllerBase
    {
        private readonly IQueueService _service;

        public QueueController(IQueueService service)
        {
            _service = service;
        }

        // POST api/queue
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateQueueRequest request)
        {
            var userId = CurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _service.CreateQueue(request.Name, userId.Value);
                return Created("/api/queue/mine", result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse(ex.Message));
            }
        }

        // POST api/queue/join
        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] JoinQueueRequest request)
        {
            var userId = CurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                return Ok(await _service.JoinQueue(request.AccessCode, userId.Value));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
        }

        // GET api/queue/mine
        [HttpGet("mine")]
        public async Task<IActionResult> MyQueues()
        {
            var userId = CurrentUserId();
            return userId == null
                ? Unauthorized()
                : Ok(await _service.GetMyQueues(userId.Value));
        }

        // POST api/queue/{queueId}/generate
        [HttpPost("{queueId:int}/generate")]
        public async Task<IActionResult> Generate(int queueId)
        {
            var userId = CurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _service.GenerateToken(queueId, userId.Value);
                return CreatedAtAction(nameof(GetStatus), new { queueId, tokenNo = result.TokenNo }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse(ex.Message));
            }
        }

        // POST api/queue/{queueId}/call-next/{tokenNo}
        [HttpPost("{queueId:int}/call-next/{tokenNo}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CallNext(int queueId, string tokenNo, [FromBody] CallNextRequest request)
        {
            var userId = CurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _service.CallNext(queueId, tokenNo, request.CounterId, userId.Value);
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
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(ex.Message));
            }
        }

        // PUT api/queue/{queueId}/complete/{tokenNo}
        [HttpPut("{queueId:int}/complete/{tokenNo}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Complete(int queueId, string tokenNo)
        {
            var userId = CurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _service.CompleteToken(queueId, tokenNo, userId.Value);
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
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(ex.Message));
            }
        }

        // GET api/queue/{queueId}/waiting
        [HttpGet("{queueId:int}/waiting")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Waiting(int queueId)
        {
            var userId = CurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                return Ok(await _service.GetWaitingQueue(queueId, userId.Value));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(ex.Message));
            }
        }

        // GET api/queue/{queueId}/status/{tokenNo}
        [HttpGet("{queueId:int}/status/{tokenNo}")]
        public async Task<IActionResult> GetStatus(int queueId, string tokenNo)
        {
            var userId = CurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _service.GetTokenStatus(queueId, tokenNo, userId.Value);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        private int? CurrentUserId()
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                ? userId
                : null;
        }
    }
}