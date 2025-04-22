namespace CallRecordIntelligence.API.Services;

public class DependencyRegistration
{
    public static IServiceCollection RegisterDependency(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IStatisticService, StatisticService>();
        services.AddScoped<ICallRecordService, CallRecordService>();
        
        return services;
    }
}