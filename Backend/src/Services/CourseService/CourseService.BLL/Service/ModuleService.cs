using CourseService.BLL.Interface;
using CourseService.DAL.Repo;
using DTOs._2CourseService;
using System.Net.Http.Json;

namespace CourseService.BLL.Service
{
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepo;
        private readonly HttpClient _assessmentClient;

        public ModuleService(
            IModuleRepository moduleRepo,
            IHttpClientFactory httpFactory)
        {
            _moduleRepo = moduleRepo;
            _assessmentClient = httpFactory.CreateClient("AssessmentService");
        }


        /// Get CourseId from ModuleId
        public async Task<ModuleAndCourseIdDTO?> GetModuleAndCourseIdAsync(int moduleId)
        {
            var module = await _moduleRepo.GetByIdAsync(moduleId);
            if (module == null)
                return null;

            return new ModuleAndCourseIdDTO
            {
                CourseId = module.CourseId,
                ModuleId = module.Id
            };
        }



        // -----------------------------
        // GET MODULES BY COURSE
        // -----------------------------
        public async Task<IEnumerable<ModuleSummaryDto>> GetModulesByCourseAsync(int courseId)
        {
            var modules = await _moduleRepo.GetByCourseIdAsync(courseId);

            return modules.Select(m => new ModuleSummaryDto
            {
                Id = m.Id,
                Title = m.Title
            });
        }

        // -----------------------------
        // GET MODULE CONTENT + QUIZ ID
        // -----------------------------
        public async Task<ModuleContentResponseDto?> GetModuleContentAsync(int moduleId)
        {
            var module = await _moduleRepo.GetByIdAsync(moduleId);
            if (module == null) return null;

            int quizId = await FetchQuizIdForModule(moduleId);

            return new ModuleContentResponseDto
            {
                Id = module.Id,
                Title = module.Title,
                Content = module.Content,
                QuizId = quizId
            };
        }

        // -----------------------------
        // HELPER: GET QUIZ ID FROM ASSESSMENT SERVICE
        // -----------------------------
        private async Task<int> FetchQuizIdForModule(int moduleId)
        {
            try
            {
                return await _assessmentClient.GetFromJsonAsync<int>($"api/quizzes/module/{moduleId}");
            }
            catch
            {
                return 0; // no quiz found
            }
        }
    }
}
