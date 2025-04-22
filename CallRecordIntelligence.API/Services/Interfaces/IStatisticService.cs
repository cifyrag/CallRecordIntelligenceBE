namespace CallRecordIntelligence.API.Services.Interfaces;

public interface IStatisticService
{
    Task<Result<decimal>> GetAverageCallCostAsync(StatisticsFilterDto filter);
    Task<Result<int>> GetTotalCallCountAsync(StatisticsFilterDto filter);
    Task<Result<TimeSpan>> GetAverageCallDurationAsync(StatisticsFilterDto filter);
    Task<Result<IEnumerable<CallRecord>>> GetLongestCallsAsync(int count, StatisticsFilterDto filter);
    Task<Result<double>> GetCallsPerPeriodAsync(StatisticsPerPeriodFilterDto filter);
    Task<Result<IEnumerable<CallVolumeDataPointDto>>> GetCallVolumeTrendAsync(StatisticsVolumeTrendFilterDto filter);
    Task<Result<Dictionary<string, decimal>>> GetTotalCostByCurrencyAsync(StatisticsFilterDto filter);
}