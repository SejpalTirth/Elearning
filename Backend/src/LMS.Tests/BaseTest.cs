using AssessmentService.DAL;
using CourseService.DAL.Models;
using GatewayService.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.DAL.Data;
using ProgresService.DAL.Data;
using UserService.DAL.Models;

namespace LMS.Tests
{
    public abstract class BaseTest
    {
        protected readonly ServiceProvider _provider;

        // All 6 DbContexts
        protected CourseContext CourseContext { get; private set; }
        protected GatewayServiceContext GatewayContext { get; private set; }
        protected UserContext UserContext { get; private set; }
        protected AssessmentDbContext AssessmentContext { get; private set; }
        protected ProgressDbContext ProgressContext { get; private set; }
        protected NotificationDbContext NotificationContext { get; private set; }

        protected BaseTest()
        {
            // Step 1: Build ServiceCollection
            var services = new ServiceCollection();
            services.AddEntityFrameworkInMemoryDatabase();

            // Step 2: Allow derived classes (Gateway tests) to register special dependencies
            ConfigureGatewayDependencies(services);

            // Step 3: Build ServiceProvider
            _provider = services.BuildServiceProvider();

            // Step 4: Generate fresh database name for each test
            string uniqueDbName() => Guid.NewGuid().ToString();

            // Step 5: Build DbContextOptions for all 6 contexts
            var courseOptions = new DbContextOptionsBuilder<CourseContext>()
                .UseInMemoryDatabase(uniqueDbName())
                .UseInternalServiceProvider(_provider)
                .Options;

            var gatewayOptions = new DbContextOptionsBuilder<GatewayServiceContext>()
                .UseInMemoryDatabase(uniqueDbName())
                .UseInternalServiceProvider(_provider)
                .Options;

            var userOptions = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(uniqueDbName())
                .UseInternalServiceProvider(_provider)
                .Options;

            var assessmentOptions = new DbContextOptionsBuilder<AssessmentDbContext>()
                .UseInMemoryDatabase(uniqueDbName())
                .UseInternalServiceProvider(_provider)
                .Options;

            var progressOptions = new DbContextOptionsBuilder<ProgressDbContext>()
                .UseInMemoryDatabase(uniqueDbName())
                .UseInternalServiceProvider(_provider)
                .Options;

            var notificationOptions = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseInMemoryDatabase(uniqueDbName())
                .UseInternalServiceProvider(_provider)
                .Options;

            // Step 6: Create actual DbContext instances
            CourseContext = new CourseContext(courseOptions);
            GatewayContext = new GatewayServiceContext(gatewayOptions);
            UserContext = new UserContext(userOptions);
            AssessmentContext = new AssessmentDbContext(assessmentOptions);
            ProgressContext = new ProgressDbContext(progressOptions);
            NotificationContext = new NotificationDbContext(notificationOptions);
        }

        /// <summary>
        /// Override this only in Gateway test classes to inject special mocks:
        /// - IHttpClientFactory
        /// - Fake HttpMessageHandlers
        /// - Additional gateway services
        /// </summary>
        protected virtual void ConfigureGatewayDependencies(IServiceCollection services)
        {
            // Default: nothing added.
            // Gateway tests will override this method.
        }
    }

}