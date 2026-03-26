using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WorkflowService.Services;
using WorkflowService.Entitites;
using WorkflowService.Data;
namespace WorkflowService.Controller
{
    
    [ApiController]
    [Route("api/workflow")]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _service;

        public WorkflowController(IWorkflowService service)
        {
            _service = service;
        }

        [Authorize(Roles = "ContentExecutive,Admin")]
        [HttpPost("submit/{productId}")]
        public async Task<IActionResult> Submit(Guid productId)
        {
            return Ok(await _service.Submit(productId));
        }

        [Authorize(Roles = "ProductManager,Admin")]
        [HttpPost("approve/{productId}")]
        public async Task<IActionResult> Approve(Guid productId)
        {
            return Ok(await _service.Approve(productId));
        }

        [Authorize(Roles = "ProductManager,Admin")]
        [HttpPost("reject/{productId}")]
        public async Task<IActionResult> Reject(Guid productId)
        {
            return Ok(await _service.Reject(productId));
        }
    }
}
