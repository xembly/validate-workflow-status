using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ValidateWorkflow.Models;
using Xunit;

namespace ValidateWorkflow.Tests
{
	public class ProgramTests
	{
		[Fact]
		public async Task CanRunMain()
		{
			Program.Run = services =>
			{
				var settings = (Settings) services.GetService(typeof(Settings));
				settings.Project.Should().Be("test/project");
				settings.Token.Should().Be("authToken");
				settings.Workflow.Should().Be("build");
				settings.Branch.Should().Be("main");
				settings.Status.Should().Be("success");

				return Task.CompletedTask;
			};

			await Program.Main(new[]
			{
				"test/project",
				"authToken",
				"build",
				"main",
				"success"
			});
		}

		[Fact]
		public async Task ExecuteRun()
		{
			// setup
			var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
			var app = fixture.Freeze<Mock<IApp>>();
			var settings = fixture.Create<Settings>();
			var serviceProvider = fixture.Create<Mock<IServiceProvider>>();
			var writeLine = new Mock<Action<string>>();
			var githubActionService = new GithubActionService
			{
				WriteLine = writeLine.Object
			};
			fixture.Register<IGithubActionService>(() => githubActionService);
			serviceProvider
				.Setup(x => x.GetService(It.Is<Type>(x => x == typeof(Settings))))
				.Returns(() => settings);
			serviceProvider
				.Setup(x => x.GetService(It.Is<Type>(x => x == typeof(IApp))))
				.Returns(() => app.Object);
			serviceProvider
				.Setup(x => x.GetService(It.Is<Type>(x => x == typeof(IGithubActionService))))
				.Returns(() => githubActionService);

			// test
			await Program.Run(serviceProvider.Object);

			// assertions
			app.Verify(x => x.ValidateStatus(), Times.Once);
			writeLine.Verify(x => x(It.Is<string>(m => m == "::debug::Settings: " + JsonConvert.SerializeObject(settings))), Times.Once);
		}
	}
}
