using Xunit;
using NetArchTest.Rules;
using System.Reflection;
using BanhCanhCaLoc.Domain.Common;
using BanhCanhCaLoc.Application.Common.Messaging;
using BanhCanhCaLoc.Infrastructure.Persistence;

namespace BanhCanhCaLoc.ArchTests
{
    public class ArchitectureTests
    {
        private static readonly Assembly DomainAssembly = typeof(BaseEntity<>).Assembly;
        private static readonly Assembly ApplicationAssembly = typeof(ICommand).Assembly;
        private static readonly Assembly InfrastructureAssembly = typeof(BanhCanhCaLocDbContext).Assembly;

        [Fact]
        public void Domain_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var otherProjects = new[]
            {
                "BanhCanhCaLoc.Application",
                "BanhCanhCaLoc.Infrastructure",
                "BanhCanhCaLoc.Api",
                "BanhCanhCaLoc.Contracts"
            };

            // Act
            var result = Types.InAssembly(DomainAssembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherProjects)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, "Domain layer should not depend on other layers");
        }

        [Fact]
        public void Application_Should_Not_HaveDependencyOnInfrastructureOrApi()
        {
            // Arrange
            var forbiddenProjects = new[]
            {
                "BanhCanhCaLoc.Infrastructure",
                "BanhCanhCaLoc.Api"
            };

            // Act
            var result = Types.InAssembly(ApplicationAssembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenProjects)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, "Application layer should not depend on Infrastructure or Api layers");
        }

        [Fact]
        public void Infrastructure_Should_Not_HaveDependencyOnApi()
        {
            // Arrange
            var forbiddenProjects = new[]
            {
                "BanhCanhCaLoc.Api"
            };

            // Act
            var result = Types.InAssembly(InfrastructureAssembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenProjects)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, "Infrastructure layer should not depend on Api layer");
        }
    }
}
