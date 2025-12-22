using CourseService.DAL.Models;
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
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }
    }
}
