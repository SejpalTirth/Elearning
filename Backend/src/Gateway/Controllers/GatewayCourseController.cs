using DTOs._2CourseService;
using Gateway.Contracts.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/course")]
    public class GatewayCourseController : BaseGatewayController
    {
        private const string BASE = "api/courses";
        private const string gateway_error_message = "Gateway error";

        public GatewayCourseController(IHttpClientFactory factory)
            : base(factory.CreateClient("CourseService"))
        {
        }

        // ------------------- COURSES -------------------

        [AllowAnonymous]
        [HttpPost("all")]
        [ProducesResponseType(typeof(IEnumerable<CourseResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseResponseDto>>> GetAll()
        {
            try
            {
                var result = await ForwardPost($"{BASE}/all", new { });

                if (result == null) return NotFound("Courses not found");

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }

                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = gateway_error_message, details = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("by-id")]
        [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CourseResponseDto>> GetById([FromBody] CourseIdRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/by-id", request);

                if (result == null)
                {
                    return NotFound("Course not found");
                }

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }

                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "gateway_error_message", details = ex.Message });
            }
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(Course), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Course>> Create(
            [FromBody] CourseCreateRequest dto)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/create", dto);

                if (result == null)
                {
                    return StatusCode(500, new { message = "Course creation failed" });
                }

                if (result is CreatedResult created)
                {
                    return Created(
                        created.Location ?? string.Empty,
                        created.Value
                    );
                }

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }

                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }


        [HttpPost("update")]
        [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Course>> Update(
            [FromBody] UpdateCourseRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/update", request);

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }
                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }


        [HttpPost("delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(
    [FromBody] CourseIdRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/delete", request);

                if (result is NoContentResult)
                {
                    return NoContent();
                }

                return result as IActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }

        // ------------------- INSTRUCTOR -------------------

        [HttpPost("instructor")]
        [ProducesResponseType(typeof(IEnumerable<Course>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Course>> GetByInstructor()
        {
            try
            {
                var result = await ForwardPost($"{BASE}/instructor", new { });

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }
                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }

        [HttpPost("unfinished")]
        [ProducesResponseType(typeof(IEnumerable<Course>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Course>> GetUnfinishedCourses()
        {
            try
            {
                var result = await ForwardPost($"{BASE}/unfinished", new { });

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }
                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }

        [HttpPost("continue")]
        [ProducesResponseType(typeof(IEnumerable<Course>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Course>> Continue(
            [FromBody] ContinueCourseRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/continue", new { });

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }
                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }

        // ------------------- ENROLLMENT -------------------

        [HttpPost("enroll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Enroll([FromBody] EnrollRequest dto)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/enroll", dto);
                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }
                return result as IActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("enrolled")]
        [ProducesResponseType(typeof(IEnumerable<EnrolledCourseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EnrolledCourseResponseDto>>> GetEnrolled()
        {
            try
            {
                var result = await ForwardPost($"{BASE}/enrolled", new { });

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }

                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }



        // ------------------- MODULES -------------------

        [HttpPost("modules")]
        [ProducesResponseType(typeof(IEnumerable<ModuleSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetModules(
            [FromBody] CourseIdRequest request)
        {
            try
            {
                var result = await ForwardPost("api/modules/by-course",request);

                if (result == null) return NotFound("Modules not found");

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }

                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = gateway_error_message, details = ex.Message });
            }
        }

        [HttpPost("module")]
        [ProducesResponseType(typeof(ModuleContentResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetModule(
            [FromBody] ModuleIdRequest request)
        {
            try
            {
                var result = await ForwardPost("api/modules/content", request);

                if (result == null) return NotFound("Module not found");

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }

                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = gateway_error_message, details = ex.Message });
            }
        }

        // ------------------- CATEGORIES -------------------

        [AllowAnonymous]
        [HttpPost("categories")]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
        {
            try
            {
                var result = await ForwardPost("api/categories", new { });

                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }

                return result as ActionResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }

        // ------------------- PUBLISH / RESTORE -------------------

        [HttpPost("publish")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PublishCourse(
            [FromBody] CourseIdRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/publish", request);
                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }
                return result as IActionResult;

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }

        [HttpPost("restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Restore(
            [FromBody] CourseIdRequest request)
        {
            try
            {
                var result = await ForwardPost($"{BASE}/restore", request);
                if (result is OkObjectResult ok)
                {
                    return Ok(ok.Value);
                }
                return result as IActionResult;
            }
            catch(Exception ex)
            {
                return StatusCode(500, new
                {
                    message = gateway_error_message,
                    details = ex.Message
                });
            }
        }
    }
}
