using CourseService.DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CourseService.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        static private List<course> courses = new List<course>
        {
            new course
            {
                ID = 1,
                CourseName = "Introduction to dotnet",
                CourseDescription = "This course helps you learn dotnet from the scratch."
            },
            new course
            {
                ID = 2,
                CourseName = "Dotnet intermediate",
                CourseDescription = "This course is all about developing and practicing dotnet basics and principles"
            },
            new course
            {
                ID = 3,
                CourseName = "Mastering dotnet",
                CourseDescription = "This course helps you with building some complex projects with ease."
            }
        };

        [HttpGet]
        public ActionResult<List<course>> Get()
        {
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public ActionResult<course> Get(int id)
        {
            var c = courses.FirstOrDefault(c => c.ID == id);
            return Ok(c);
        }
    }
}
