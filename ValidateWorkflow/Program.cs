using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ValidateWorkflow.Models;

namespace ValidateWorkflow
{
	internal static class Program
	{
		public static Func<IServiceProvider, Task> Run = async serviceProvider =>
		{
			var app = serviceProvider.GetService<IApp>();
			var actionService = serviceProvider.GetService<IGithubActionService>();
			var settings = JsonConvert.SerializeObject(serviceProvider.GetService<Settings>());

			actionService.LogDebug("Settings: " + settings);
			await app.ValidateStatus();
		};

		public static async Task Main(string[] args)
		{
//#if DEBUG
//			Console.WriteLine("Waiting for debugger to attach");
//			while (!System.Diagnostics.Debugger.IsAttached)
//			{
//				await Task.Delay(100);
//			}
//			Console.WriteLine("Debugger attached");
//#endif
			var services = new ServiceCollection();
			services.AddTransient(_ => new Settings
			{
				Project   = args[0],
				Token     = args[1],
				Workflow  = args[2],
				Branch    = args[3],
				Status    = args[4]
			});
			services.AddTransient<IApp, App>();
			services.AddTransient<IGithubApiService, GithubApiService>();
			services.AddTransient<IGithubActionService, GithubActionService>();

			using var serviceProvider = services.BuildServiceProvider();
			await Run(serviceProvider);
		}
	}
}
