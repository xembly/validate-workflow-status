using System.Threading.Tasks;

namespace ValidateWorkflow
{
	internal interface IGithubApiService
	{
		Task<int> GetWorkflowId();
		Task<string> GetWorkflowStatus(int workflowId);
	}
}
