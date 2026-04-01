using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminService.Services;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;

        public AdminController(IAdminService service)
        {
            _service = service;
        }

        // 🔥 DASHBOARD KPI
        [HttpGet("reports/dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            return Ok(await _service.GetDashboard());
        }

        // 🔥 PRODUCT AUDIT
        [HttpGet("audit/products/{id}")]
        public async Task<IActionResult> GetAudit(Guid id)
        {
            return Ok(await _service.GetAudit(id));
        }
    }
}