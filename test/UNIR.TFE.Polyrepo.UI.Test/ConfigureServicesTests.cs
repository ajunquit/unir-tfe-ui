using Microsoft.Extensions.DependencyInjection;
using UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub;
using UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub.Impl;

namespace UNIR.TFE.Polyrepo.UI.Test
{
    public class InfrastructureConfigureServicesTests
    {
        public static IEnumerable<object[]> ServicePairs => new[]
        {
            new object[] { typeof(IGitRepositoryAnalyzerService), typeof(GitRepositoryAnalyzerService) },
            new object[] { typeof(IGitHubRepositoryService),      typeof(GitHubRepositoryService) },
            new object[] { typeof(IGitHubUrlParser),              typeof(GitHubUrlParser) }
        };

        private static ServiceDescriptor FindDescriptor(ICollection<ServiceDescriptor> services, Type serviceType)
            => Assert.Single(services, d => d.ServiceType == serviceType);

        private static void AssertServiceRegisteredAsScoped(
            IServiceCollection services, Type serviceType, Type implementationType)
        {
            var descriptor = FindDescriptor((ICollection<ServiceDescriptor>)services, serviceType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
            Assert.Equal(implementationType, descriptor.ImplementationType);
        }

        // 1) Registra cada servicio con implementación y lifetime Scoped
        [Theory]
        [MemberData(nameof(ServicePairs))]
        public void AddInfrastructureModule_RegistersScopedWithExpectedImplementation(Type serviceType, Type implType)
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddInfrastructureModule();

            // Assert
            AssertServiceRegisteredAsScoped(services, serviceType, implType);
        }

        // 2) (Comportamiento actual con AddScoped) Llamar dos veces duplica registros
        //    Si migras a TryAddScoped, cambia el Assert.Equal(2, ...) por Assert.Equal(1, ...)
        [Fact]
        public void AddInfrastructureModule_WhenCalledTwice_DuplicatesDescriptorsWithAddScoped()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddInfrastructureModule();
            services.AddInfrastructureModule();

            // Assert
            var types = new[]
            {
                typeof(IGitRepositoryAnalyzerService),
                typeof(IGitHubRepositoryService),
                typeof(IGitHubUrlParser)
            };

            foreach (var svcType in types)
            {
                var count = services.Count(d => d.ServiceType == svcType);
                Assert.Equal(2, count); // Cambia a 1 si usas TryAddScoped en el módulo
            }
        }
    }
}
