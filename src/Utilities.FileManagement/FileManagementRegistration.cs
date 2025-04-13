using Microsoft.Extensions.DependencyInjection;
using Utilities.Gpg;
using Utilities.IoOperations;

namespace Utilities.FileManagement;

public static class FileManagementRegistration
{
	public static IServiceCollection AddFileManagementServices(this IServiceCollection services)
	{
		services.AddIoOperationsServices();
		services.AddGpgServices();
		return services;
	}
}
