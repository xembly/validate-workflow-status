using System.Threading.Tasks;

namespace ValidateWorkflow
{
	internal interface IApp
	{
		Task ValidateStatus();
	}
}