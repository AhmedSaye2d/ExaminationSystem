using Exam.Application.Dto.Course;
using Exam.Application.Services.Interfaces.ICourseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exam.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Retrieve all courses.
        /// </summary>
        /// <returns>A list of courses.</returns>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _courseService.GetAllAsync();
            return Ok(res);
        }

        /// <summary>
        /// Get details of a specific course by ID.
        /// </summary>
        /// <param name="id">Course ID.</param>
        /// <returns>The course details.</returns>
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _courseService.GetByIdAsync(id);
            return Ok(res);
        }

        /// <summary>
        /// [Admin Only] Create a new course.
        /// </summary>
        /// <param name="dto">Course creation data.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CourseCreateDTO dto)
        {
            await _courseService.CreateAsync(dto);
            return Ok(new { message = "Course created successfully" });
        }

        /// <summary>
        /// [Admin Only] Update an existing course.
        /// </summary>
        /// <param name="id">Course ID to update.</param>
        /// <param name="dto">Updated course data.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CourseCreateDTO dto)
        {
            await _courseService.UpdateAsync(id, dto);
            return Ok(new { message = "Course updated successfully" });
        }

        /// <summary>
        /// [Admin Only] Delete a course by ID.
        /// </summary>
        /// <param name="id">Course ID to delete.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseService.DeleteAsync(id);
            return Ok(new { message = "Course deleted successfully" });
        }

        /// <summary>
        /// Retrieve all exams associated with a specific course.
        /// </summary>
        /// <param name="courseId">Course ID.</param>
        /// <returns>A list of exams for the course.</returns>
        [HttpGet("{courseId:int}/exams")]
        public async Task<IActionResult> GetCourseExams(int courseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            
            var exams = await _courseService.GetCourseExamsAsync(courseId, userId, role);
            return Ok(exams);
        }

        /// <summary>
        /// Retrieve all students enrolled in a specific course.
        /// </summary>
        /// <param name="courseId">Course ID.</param>
        /// <returns>A list of students enrolled in the course.</returns>
        [HttpGet("{courseId:int}/students")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetCourseStudents(int courseId)
        {
            var students = await _courseService.GetCourseStudentsAsync(courseId);
            return Ok(students);
        }

        /// <summary>
        /// [Admin Only] Assign an instructor to teach a specific course.
        /// </summary>
        /// <param name="courseId">Course ID.</param>
        /// <param name="instructorId">Instructor ID to assign.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("{courseId:int}/assign-instructor")]
        public async Task<IActionResult> AssignInstructor(int courseId, [FromQuery] int instructorId)
        {
            await _courseService.AssignInstructorToCourseAsync(courseId, instructorId);
            return Ok(new { message = "Instructor assigned successfully" });
        }
    }
}
