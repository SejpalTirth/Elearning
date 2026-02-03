using CourseService.DAL.Models;
using DTOs._2CourseService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly CourseContext _context;

        public CategoriesController(CourseContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }
    }
}
