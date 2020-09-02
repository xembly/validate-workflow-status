using System;
using System.Threading.Tasks;
using ValidateWorkflow.Models;

namespace ValidateWorkflow
{
	internal class App : IApp
	{
		private readonly Settings _settings;
		private readonly IGithubApiService _githubApiService;
		private readonly IGithubActionService _githubActionService;

		public App(Settings settings, IGithubApiService githubApiService, IGithubActionService githubActionService)
		{
			_settings = settings;
			_githubApiService = githubApiService;
			_githubActionService = githubActionService;
		}

		public async Task ValidateStatus()
		{
			int workflowId = default;
			string workflowStatus = default;

			try
			{
				workflowId = await _githubApiService.GetWorkflowId();
				_githubActionService.LogDebug($"Workflow Id: {workflowId}");
			}
			catch (Exception)
			{
				_githubActionService.LogFatal($"Unable to find workflow with the name {_settings.Workflow}");
			}

			try
			{
				workflowStatus = await _githubApiService.GetWorkflowStatus(workflowId);
				_githubActionService.LogDebug($"Workflow Status: {workflowStatus}");
			}
			catch (Exception)
			{
				_githubActionService.LogFatal($"Unable to find last workflow run for {_settings.Workflow}");
			}

			if (!string.Equals(workflowStatus, _settings.Status, StringComparison.InvariantCultureIgnoreCase))
			{
				_githubActionService.LogFatal($"Latest workflow run returned a status of {workflowStatus} rather than expected {_settings.Status}");
			}
			_githubActionService.LogDebug($"Latest workflow run returned a status of {workflowStatus}; expected {_settings.Status}");
		}
	}
}
