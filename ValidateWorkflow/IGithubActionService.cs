namespace ValidateWorkflow
{
	internal interface IGithubActionService
	{
		void LogDebug(string msg);
		void LogError(string msg, string file = null, string line = null, string col = null);
		void LogFatal(string msg, string file = null, string line = null, string col = null);
		void LogWarning(string msg, string file = null, string line = null, string col = null);
		void SetOutput(string name, string value);
	}
}