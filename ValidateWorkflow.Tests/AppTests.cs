using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using System;
using System.Threading.Tasks;
using ValidateWorkflow.Models;
using Xunit;

namespace ValidateWorkflow.Tests
{
	public class AppTests
	{
		private readonly IFixture _fixture;
		private readonly Mock<Action<string>> _writeLine;

		public AppTests()
		{
			_writeLine = new Mock<Action<string>>();
			_fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

			_fixture.Register<IGithubActionService>(() => new GithubActionService
			{
				WriteLine = _writeLine.Object,
				FatalAction = () => { }
			});

			_fixture.Register(() => new Settings
			{
				Project = "test/project",
				Token = "authToken",
				Workflow = "build",
				Branch = "main",
				Status = "success"
			});

			var githubApiService = _fixture.Freeze<Mock<IGithubApiService>>();
			githubApiService
				.Setup(x => x.GetWorkflowId())
				.Returns(() => Task.FromResult(10));
			githubApiService
				.Setup(x => x.GetWorkflowStatus(It.IsAny<int>()))
				.Returns(() => Task.FromResult("success"));
		}

		[Fact]
		public async Task CanValidateStatus()
		{
			// setup
			var sut = _fixture.Create<App>();
			var githubApiService = _fixture.Create<Mock<IGithubApiService>>();

			// test
			await sut.ValidateStatus();

			// assertions
			githubApiService.Verify(x => x.GetWorkflowId(), Times.Once);
			githubApiService.Verify(x => x.GetWorkflowStatus(10), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::debug::Workflow Id: 10")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::debug::Workflow Status: success")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::debug::Latest workflow run returned a status of success; expected success")), Times.Once);
		}

		[Fact]
		public async Task FailValidateStatus_NoWorkflowFound()
		{
			// setup
			var githubApiService = _fixture.Create<Mock<IGithubApiService>>();
			githubApiService
				.Setup(x => x.GetWorkflowId())
				.Throws(new Exception());

			var sut = _fixture.Create<App>();

			// test
			await sut.ValidateStatus();

			// assertions
			githubApiService.Verify(x => x.GetWorkflowId(), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error ::Unable to find workflow with the name build")), Times.Once);
		}

		[Fact]
		public async Task FailValidateStatus_NoWorkflowRuns()
		{
			// setup
			var githubApiService = _fixture.Create<Mock<IGithubApiService>>();
			githubApiService
				.Setup(x => x.GetWorkflowStatus(10))
				.Throws(new Exception());

			var sut = _fixture.Create<App>();

			// test
			await sut.ValidateStatus();

			// assertions
			githubApiService.Verify(x => x.GetWorkflowId(), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::debug::Workflow Id: 10")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error ::Unable to find last workflow run for build")), Times.Once);
		}

		[Fact]
		public async Task FailValidateStatus_BadWorkflowStatus()
		{
			// setup
			var githubApiService = _fixture.Create<Mock<IGithubApiService>>();
			githubApiService
				.Setup(x => x.GetWorkflowStatus(10))
				.Returns(() => Task.FromResult("failed"));

			var sut = _fixture.Create<App>();

			// test
			await sut.ValidateStatus();

			// assertions
			githubApiService.Verify(x => x.GetWorkflowId(), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::debug::Workflow Id: 10")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::debug::Workflow Status: failed")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error ::Latest workflow run returned a status of failed rather than expected success")), Times.Once);
		}
	}
}
