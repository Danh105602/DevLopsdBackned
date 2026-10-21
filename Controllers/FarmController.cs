using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FarmController : ControllerBase
    {
        private readonly IFarmService _service;

        public FarmController(IFarmService service)
        {
            _service = service;
        }

        // GET: api/Farm
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var farms = await _service.GetAllAsync();
            return Ok(farms);
        }

        // GET: api/Farm/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var farm = await _service.GetByIdAsync(id);

            if (farm == null)
                return NotFound(new { message = "Không tìm thấy trang trại" });

            return Ok(farm);
        }

        // POST: api/Farm
        [HttpPost]
        public async Task<IActionResult> Create(Farm farm)
        {
            var createdFarm = await _service.CreateAsync(farm);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdFarm.Id },
                createdFarm);
        }

        // PUT: api/Farm/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Farm farm)
        {
            var updatedFarm = await _service.UpdateAsync(id, farm);

            if (updatedFarm == null)
                return NotFound(new { message = "Không tìm thấy trang trại" });

            return Ok(updatedFarm);
        }

        // DELETE: api/Farm/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(new { message = "Không tìm thấy trang trại" });

            return Ok(new { message = "Xóa trang trại thành công" });
        }
    }
}