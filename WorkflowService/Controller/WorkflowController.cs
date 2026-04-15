using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowService.Data;
using WorkflowService.Dtos;
using WorkflowService.Entities;
using WorkflowService.Services;
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

        [HttpPost("publish/{id}")]
        public async Task<IActionResult> Publish(Guid id)
        {
            return Ok(await _service.Publish(id));
        }

        [HttpGet("history/{id}")]
        public async Task<IActionResult> GetHistory(Guid id)
        {
            return Ok(await _service.GetHistory(id));
        }


        [HttpPut("products/{id}/pricing")]
        public async Task<IActionResult> UpdatePricing(Guid id, PricingDto dto)
        {
            return Ok(await _service.UpdatePricing(id, dto));
        }

        [HttpPut("products/{id}/inventory")]
        public async Task<IActionResult> UpdateInventory(Guid id, InventoryDto dto)
        {
            return Ok(await _service.UpdateInventory(id, dto));
        }
    }
}
