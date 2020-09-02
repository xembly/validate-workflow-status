using Flurl.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using ValidateWorkflow.Models;

namespace ValidateWorkflow
{
	internal class GithubApiService : IGithubApiService
	{
		private readonly Settings _settings;
		private readonly IGithubActionService _githubActionService;

		public GithubApiService(Settings settings, IGithubActionService githubActionService)
		{
			_settings = settings;
			_githubActionService = githubActionService;
		}

		public async Task<int> GetWorkflowId()
		{
			var response = await $"https://api.github.com/repos/{_settings.Project}/actions/workflows"
				.WithHeader("Authorization", $"token {_settings.Token}")
				.WithHeader("User-Agent", "Flurl")
				.GetJsonAsync<WorkflowResponse>();
			_githubActionService.LogDebug($"Workflows Found: {response?.TotalCount}");
			return response.Workflows
				.First(w => string.Equals(w.Name, _settings.Workflow, StringComparison.InvariantCultureIgnoreCase))
				.Id;
		}

		public async Task<string> GetWorkflowStatus(int workflowId)
		{
			var response = await $"https://api.github.com/repos/{_settings.Project}/actions/workflows/{workflowId}/runs"
				.WithHeader("Authorization", $"token {_settings.Token}")
				.WithHeader("User-Agent", "Flurl")
				.SetQueryParams(new
				{
					branch = _settings.Branch,
					page = 1,
					per_page = 10
				})
				.GetJsonAsync<WorkflowRunsResponse>();
			_githubActionService.LogDebug($"Workflow Runs Found: {response?.TotalCount}");
			return response.WorkflowRuns
				.First(w => string.Equals(w.Status, "completed", StringComparison.InvariantCultureIgnoreCase))
				.Conclusion;
		}
	}
}
