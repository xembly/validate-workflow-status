using System;
using System.Collections.Generic;
using System.Linq;

namespace ValidateWorkflow
{
	internal class GithubActionService : IGithubActionService
	{
		internal Action FatalAction = () => Environment.Exit(1);
		internal Action<string> WriteLine = msg => Console.WriteLine(msg);

		public void SetOutput(string name, string value)
		{
			WriteLine($"::set-output name={name}::{value}");
		}

		public void LogDebug(string msg)
		{
			WriteLine($"::debug::{msg}");
		}

		public void LogWarning(string msg, string file = null, string line = null, string col = null)
		{
			var opts = string.Join(',', new Dictionary<string, string>()
			{
				{ "file", file },
				{ "line", line },
				{ "col", col }
			}
				.Where(d => !string.IsNullOrWhiteSpace(d.Value))
				.Select(d => $"{d.Key}={d.Value}")).Trim();
			WriteLine($"::warning {opts}::{msg}");
		}

		public void LogError(string msg, string file = null, string line = null, string col = null)
		{
			var opts = string.Join(',', new Dictionary<string, string>()
			{
				{ "file", file },
				{ "line", line },
				{ "col", col }
			}
				.Where(d => !string.IsNullOrWhiteSpace(d.Value))
				.Select(d => $"{d.Key}={d.Value}")).Trim();
			WriteLine($"::error {opts}::{msg}");
		}

		public void LogFatal(string msg, string file = null, string line = null, string col = null)
		{
			LogError(msg, file, line, col);
			FatalAction();
		}
	}
}
