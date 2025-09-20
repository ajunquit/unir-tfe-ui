using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UNIR.TFE.Polyrepo.Addition.Module.Application;
using UNIR.TFE.Polyrepo.Division.Module.Application;
using UNIR.TFE.Polyrepo.Multiplication.Module.Application;
using UNIR.TFE.Polyrepo.Subtraction.Module.Application;
using UNIR.TFE.Polyrepo.UI.Controllers;
using UNIR.TFE.Polyrepo.UI.Infrastructure.External.GitHub;
using UNIR.TFE.Polyrepo.UI.Models;
using UNIR.TFE.Polyrepo.UI.Models.Common;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace UNIR.TFE.Polyrepo.UI.Test
{
    public class HomeControllerTests
    {
        private readonly Mock<IDivisionAppService> _division = new();
        private readonly Mock<IMultiplicationAppService> _mult = new();
        private readonly Mock<ISubtractionAppService> _sub = new();
        private readonly Mock<IAdditionAppService> _add = new();
        private readonly Mock<ILogger<HomeController>> _logger = new();
        private readonly Mock<IGitRepositoryAnalyzerService> _git = new();

        private (HomeController controller, IConfiguration config) CreateController(
            string repoUrl = "repoUrl",
            string branch = "main",
            string? token = null)
        {
            var settings = new Dictionary<string, string?>
            {
                ["Git:Superproject"] = repoUrl,
                ["Git:Branch"] = branch
            };

            if (token is not null)
                settings["Git:Token"] = token;

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _git.Setup(g => g.AnalyzeRepositoryAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(new GitModel());

            var controller = new HomeController(
                _division.Object,
                _mult.Object,
                _sub.Object,
                _add.Object,
                _logger.Object,
                _git.Object,
                configuration);

            return (controller, configuration);
        }

        // ---------- Index ----------

        [Fact]
        public async Task Index_ReturnsView_WithCalculatorModel_AndLoadsGitInfo()
        {
            // Arrange
            var (controller, _) = CreateController(repoUrl: "https://github.com/ajunquit/unir-tfe", branch: "main");

            // Act
            var result = await controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CalculatorModel>(view.Model);
            Assert.NotNull(model.GitModel);

            _git.Verify(g => g.AnalyzeRepositoryAsync(
                "https://github.com/ajunquit/unir-tfe",
                "main",
                null), Times.Once);
        }

        // ---------- Calculate: Addition ----------

        [Fact]
        public async Task Calculate_Addition_SetsResult_LogsInfo_LoadsGit()
        {
            // Arrange
            var (controller, _) = CreateController(repoUrl: "repoUrl", branch: "dev");
            _add.Setup(a => a.Execute(2m, 3m)).Returns(5m);

            var input = new CalculatorModel
            {
                Number1 = 2m,
                Number2 = 3m,
                Operation = "Addition"
            };

            // Act
            var result = await controller.Calculate(input);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", view.ViewName);
            var model = Assert.IsType<CalculatorModel>(view.Model);
            Assert.Equal(5m, model.Result);
            Assert.NotNull(model.GitModel);

            _add.Verify(a => a.Execute(2m, 3m), Times.Once);
            _sub.Verify(s => s.Execute(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
            _mult.Verify(m => m.Execute(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
            _division.Verify(d => d.Execute(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);

            _git.Verify(g => g.AnalyzeRepositoryAsync("repoUrl", "dev", null), Times.Once);
            _logger.VerifyLog(LogLevel.Information, Times.Once());
        }

        // ---------- Calculate: Subtraction ----------

        [Fact]
        public async Task Calculate_Subtraction_SetsResult()
        {
            var (controller, _) = CreateController();
            _sub.Setup(s => s.Execute(10m, 4m)).Returns(6m);

            var input = new CalculatorModel { Number1 = 10m, Number2 = 4m, Operation = "Subtraction" };

            var result = await controller.Calculate(input);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CalculatorModel>(view.Model);
            Assert.Equal(6m, model.Result);

            _sub.Verify(s => s.Execute(10m, 4m), Times.Once);
            _add.Verify(a => a.Execute(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
            _mult.Verify(m => m.Execute(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
            _division.Verify(d => d.Execute(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
        }

        // ---------- Calculate: Multiplication ----------

        [Fact]
        public async Task Calculate_Multiplication_SetsResult()
        {
            var (controller, _) = CreateController();
            _mult.Setup(m => m.Execute(3m, 4m)).Returns(12m);

            var input = new CalculatorModel { Number1 = 3m, Number2 = 4m, Operation = "Multiplication" };

            var result = await controller.Calculate(input);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CalculatorModel>(view.Model);
            Assert.Equal(12m, model.Result);

            _mult.Verify(m => m.Execute(3m, 4m), Times.Once);
        }

        // ---------- Calculate: Division ----------

        [Fact]
        public async Task Calculate_Division_SetsResult()
        {
            var (controller, _) = CreateController();
            _division.Setup(d => d.Execute(8m, 2m)).Returns(4m);

            var input = new CalculatorModel { Number1 = 8m, Number2 = 2m, Operation = "Division" };

            var result = await controller.Calculate(input);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CalculatorModel>(view.Model);
            Assert.Equal(4m, model.Result);

            _division.Verify(d => d.Execute(8m, 2m), Times.Once);
        }

        // ---------- Calculate: Operación inválida ----------

        [Fact]
        public async Task Calculate_InvalidOperation_AddsModelError_LogsError()
        {
            var (controller, _) = CreateController();

            var input = new CalculatorModel { Number1 = 1m, Number2 = 1m, Operation = "Unknown" };

            var result = await controller.Calculate(input);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", view.ViewName);
            var model = Assert.IsType<CalculatorModel>(view.Model);

            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.Values.SelectMany(v => v.Errors).Any());

            _logger.VerifyLog(LogLevel.Error, Times.Once());
            // No debe invocar ningún servicio de operación
            _add.Verify(a => a.Execute(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
            _sub.Verify(s => s.Execute(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
            _mult.Verify(m => m.Execute(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
            _division.Verify(d => d.Execute(It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
        }

        // ---------- Privacy ----------

        [Fact]
        public void Privacy_ReturnsView()
        {
            var (controller, _) = CreateController();

            var result = controller.Privacy();

            Assert.IsType<ViewResult>(result);
        }

        // ---------- Error (requiere HttpContext) ----------

        [Fact]
        public void Error_ReturnsView_WithErrorViewModel()
        {
            var (controller, _) = CreateController();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = controller.Error();

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<ErrorViewModel>(view.Model);
        }
    }

    // Helper para verificar ILogger<T> con Moq sin pelear con TState genérico
    internal static class LoggerMoqExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times)
        {
            logger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => true),
                    It.IsAny<System.Exception>(),
                    (Func<It.IsAnyType, System.Exception?, string>)It.IsAny<object>()),
                times);
        }
    }
}
