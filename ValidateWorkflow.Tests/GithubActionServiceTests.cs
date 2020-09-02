using AutoFixture.AutoMoq;
using AutoFixture;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace ValidateWorkflow.Tests
{
	public class GithubActionServiceTests
	{
		private readonly IFixture _fixture;
		private readonly Mock<Action<string>> _writeLine;
		private readonly IGithubActionService _githubActionService;
		private int _numberOfFatalExits;

		public GithubActionServiceTests()
		{
			_writeLine = new Mock<Action<string>>();
			_fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
			_githubActionService = new GithubActionService
			{
				WriteLine = _writeLine.Object,
				FatalAction = () => _numberOfFatalExits++
			};
			_fixture.Register(() => _githubActionService);
		}

		[Fact]
		public void CanSetOutput()
		{
			// test
			_githubActionService.SetOutput("alpha", "testing");
			_githubActionService.SetOutput("beta", "123");

			// assertions
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::set-output name=alpha::testing")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::set-output name=beta::123")), Times.Once);
		}

		[Fact]
		public void CanLogDebug()
		{
			// test
			_githubActionService.LogDebug("My Test");
			_githubActionService.LogDebug("this should be logged.");

			// assertions
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::debug::My Test")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::debug::this should be logged.")), Times.Once);
		}

		[Fact]
		public void CanLogWarning()
		{
			// test
			_githubActionService.LogWarning("My Test");
			_githubActionService.LogWarning("this should be logged.", "alpha.cs");
			_githubActionService.LogWarning("this should be logged.", "beta.cs", "20");
			_githubActionService.LogWarning("this should be logged.", "cappa.cs", "40", "1");
			_githubActionService.LogWarning("this should be logged.", "delta.cs", null, "10");

			// assertions
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::warning ::My Test")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::warning file=alpha.cs::this should be logged.")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::warning file=beta.cs,line=20::this should be logged.")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::warning file=cappa.cs,line=40,col=1::this should be logged.")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::warning file=delta.cs,col=10::this should be logged.")), Times.Once);
		}

		[Fact]
		public void CanLogError()
		{
			// test
			_githubActionService.LogError("My Test");
			_githubActionService.LogError("this should be logged.", "alpha.cs");
			_githubActionService.LogError("this should be logged.", "beta.cs", "20");
			_githubActionService.LogError("this should be logged.", "cappa.cs", "40", "1");
			_githubActionService.LogError("this should be logged.", "delta.cs", null, "10");

			// assertions
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error ::My Test")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error file=alpha.cs::this should be logged.")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error file=beta.cs,line=20::this should be logged.")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error file=cappa.cs,line=40,col=1::this should be logged.")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error file=delta.cs,col=10::this should be logged.")), Times.Once);
		}

		[Fact]
		public void CanLogFatal()
		{
			// test
			_githubActionService.LogFatal("My Test");
			_githubActionService.LogFatal("this should be logged.", "alpha.cs");
			_githubActionService.LogFatal("this should be logged.", "beta.cs", "20");
			_githubActionService.LogFatal("this should be logged.", "cappa.cs", "40", "1");
			_githubActionService.LogFatal("this should be logged.", "delta.cs", null, "10");

			// assertions
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error ::My Test")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error file=alpha.cs::this should be logged.")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error file=beta.cs,line=20::this should be logged.")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error file=cappa.cs,line=40,col=1::this should be logged.")), Times.Once);
			_writeLine.Verify(x => x(It.Is<string>(m => m == "::error file=delta.cs,col=10::this should be logged.")), Times.Once);
			_numberOfFatalExits.Should().Be(5);
		}
	}
}
