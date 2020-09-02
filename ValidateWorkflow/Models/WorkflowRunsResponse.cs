using Newtonsoft.Json;
using System.Collections.Generic;

namespace ValidateWorkflow.Models
{
	internal class WorkflowRunsResponse
	{
		[JsonProperty("total_count")]
		public int TotalCount { get; set; }

		[JsonProperty("workflow_runs")]
		public IEnumerable<WorkflowRun> WorkflowRuns { get; set; }

		internal class WorkflowRun
		{
			public int Id { get; set; }
			public string Status { get; set; }
			public string Conclusion { get; set; }
		}
	}
}
