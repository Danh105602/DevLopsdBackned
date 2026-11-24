using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApplication1.DTOs.AuditLogs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _service;

        public AuditLogController(IAuditLogService service)
        {
            _service = service;
        }

        // GET: api/AuditLog?userId=2&action=UPDATE&entityName=Crop&from=2026-11-01&to=2026-11-30&pageNumber=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] AuditLogQueryParameters query)
        {
            return Ok(await _service.GetAllAsync(query));
        }

        // GET: api/AuditLog/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var log = await _service.GetByIdAsync(id);

            if (log == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy nhật ký thao tác"
                });
            }

            return Ok(log);
        }
    }
}