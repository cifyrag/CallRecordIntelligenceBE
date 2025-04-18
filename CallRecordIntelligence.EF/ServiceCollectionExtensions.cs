using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CallRecordIntelligence.EF;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContextFactory<ApplicationDbContext>(options =>
		{
			options.UseNpgsql(
				configuration.GetConnectionString("CdrDB"),
				npgsqlOptions =>
				{
					npgsqlOptions.EnableRetryOnFailure(
						maxRetryCount: 5, 
						maxRetryDelay: TimeSpan.FromSeconds(10),
						errorCodesToAdd: null);
					npgsqlOptions.CommandTimeout(120);
				}
			);
		});
		
		return services;
	}
}