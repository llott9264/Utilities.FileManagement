using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utilities.Gpg;

namespace Utilities.FileManagement.Tests;

public class FileManagementRegistrationTests
{
	private readonly IConfiguration _configuration = new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>
		{
			{ "Gpg:KeyFolderPath", "Value1" }
		})
		.Build();

	[Fact]
	public void AddFileManagementServices_RegistersAllServices_CorrectlyResolvesTypes()
	{
		// Arrange
		ServiceCollection services = new();
		services.AddSingleton(_configuration);

		// Act
		services.AddFileManagementServices();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		IMediator? mediator = serviceProvider.GetService<IMediator>();
		IGpg? gpg = serviceProvider.GetService<IGpg>();

		// Assert
		Assert.NotNull(mediator);
		Assert.IsType<Mediator>(mediator);

		Assert.NotNull(gpg);
		Assert.IsType<Gpg.Gpg>(gpg);
	}

	[Fact]
	public void AddFileManagementServices_ReturnsServiceCollection()
	{
		// Arrange
		ServiceCollection services = new();

		// Act
		IServiceCollection result = services.AddFileManagementServices();

		// Assert
		Assert.Same(services, result); // Ensures the method returns the same IServiceCollection
	}


	[Fact]
	public void AddFileManagementServices_ScopedLifetime_VerifyInstanceWithinScope()
	{
		// Arrange
		ServiceCollection services = new();
		services.AddSingleton(_configuration);

		// Act
		IServiceCollection result = services.AddFileManagementServices();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// Assert
		using (IServiceScope scope = serviceProvider.CreateScope())
		{
			IMediator? service1 = scope.ServiceProvider.GetService<IMediator>();
			IMediator? service2 = scope.ServiceProvider.GetService<IMediator>();

			IGpg? service3 = scope.ServiceProvider.GetService<IGpg>();
			IGpg? service4 = scope.ServiceProvider.GetService<IGpg>();

			Assert.NotSame(service1, service2); //MediatR is Transient
			Assert.Same(service3, service4); //Gpg is Singleton
		}
	}

	[Fact]
	public void AddFileManagementServices_ScopedLifetime_VerifyInstancesAcrossScopes()
	{
		// Arrange
		ServiceCollection services = new();
		services.AddSingleton(_configuration);

		// Act
		services.AddFileManagementServices();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// Assert
		IMediator? service1, service2;
		IGpg? service3, service4;

		using (IServiceScope scope1 = serviceProvider.CreateScope())
		{
			service1 = scope1.ServiceProvider.GetService<IMediator>();
		}

		using (IServiceScope scope2 = serviceProvider.CreateScope())
		{
			service2 = scope2.ServiceProvider.GetService<IMediator>();
		}

		using (IServiceScope scope1 = serviceProvider.CreateScope())
		{
			service3 = scope1.ServiceProvider.GetService<IGpg>();
		}

		using (IServiceScope scope2 = serviceProvider.CreateScope())
		{
			service4 = scope2.ServiceProvider.GetService<IGpg>();
		}

		Assert.NotSame(service1, service2); //MediatR is Transient
		Assert.Same(service3, service4); //Gpg is Singleton
	}
}
