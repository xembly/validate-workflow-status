using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Flurl.Http.Testing;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using ValidateWorkflow.Models;
using Xunit;

namespace ValidateWorkflow.Tests
{
	public class GithubApiServiceTests : IDisposable
	{
		private readonly IFixture _fixture;
		private readonly HttpTest _httpTest;
		private readonly Settings _settings;
		private readonly Mock<Action<string>> _writeLine;
		private readonly IGithubActionService _githubActionService;

		public GithubApiServiceTests()
		{
			_httpTest = new HttpTest();
			_settings = new Settings
			{
				Project = "test/project",
				Token = "authToken",
				Workflow = "build",
				Branch = "main",
				Status = "success"
			};
			_fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
			_writeLine = new Mock<Action<string>>();
			_githubActionService = new GithubActionService
			{
				WriteLine = _writeLine.Object
			};
			_fixture.Register(() => _githubActionService);
		}

		public void Dispose() => _httpTest?.Dispose();

		[Fact]
		public async Task CanGetWorkflowId()
		{
			// setup
			_httpTest.RespondWithJson(new WorkflowResponse
			{
				TotalCount = 2,
				Workflows = new[]
				{
					new WorkflowResponse.Workflow
					{
						Id = 1,
						Name = "alpha"
					},
					new WorkflowResponse.Workflow
					{
						Id = 2,
						Name = "build"
					}
				}
			});

			// test
			var sut = new GithubApiService(_settings, _githubActionService);
			var id = await sut.GetWorkflowId();

			// assertions
			_httpTest.ShouldHaveCalled($"https://api.github.com/repos/{_settings.Project}/actions/workflows")
				.WithVerb(HttpMethod.Get)
				.WithHeader("Authorization", $"token {_settings.Token}")
				.WithHeader("User-Agent", "Flurl");
			id.Should().Be(2);
		}

		[Fact]
		public async Task CanGetWorkflowStatus()
		{
			// setup
			_httpTest.RespondWithJson(new WorkflowRunsResponse
			{
				TotalCount = 2,
				WorkflowRuns = new[]
				{
					new WorkflowRunsResponse.WorkflowRun
					{
						Id = 2,
						Status = "completed",
						Conclusion = "success"
					},
					new WorkflowRunsResponse.WorkflowRun
					{
						Id = 1,
						Status = "completed",
						Conclusion = "failed"
					}
				}
			});

			// test
			var sut = new GithubApiService(_settings, _githubActionService);
			var status = await sut.GetWorkflowStatus(1);

			// assertions
			_httpTest.ShouldHaveCalled($"https://api.github.com/repos/{_settings.Project}/actions/workflows/1/runs")
				.WithVerb(HttpMethod.Get)
				.WithHeader("Authorization", $"token {_settings.Token}")
				.WithHeader("User-Agent", "Flurl")
				.WithQueryParamValue("branch", _settings.Branch)
				.WithQueryParamValue("page", "1")
				.WithQueryParamValue("per_page", "10");
			status.Should().Be("success");
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::debug::Workflow Runs Found: 2")), Times.Once);
		}
	}
}
