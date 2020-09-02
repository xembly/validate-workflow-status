using Newtonsoft.Json;
using System.Collections.Generic;

namespace ValidateWorkflow.Models
{
	internal class WorkflowResponse
	{
		[JsonProperty("total_count")]
		public int TotalCount { get; set; }

		public IEnumerable<Workflow> Workflows { get; set; }

		internal class Workflow
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}
	}
}
