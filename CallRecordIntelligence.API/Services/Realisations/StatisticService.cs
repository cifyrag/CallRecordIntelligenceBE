using System.Linq.Expressions;
using CallRecordIntelligence.EF.Services;

namespace CallRecordIntelligence.API.Services.Realisations;

public class StatisticService: IStatisticService
{
    private readonly ILogger<StatisticService> _logger;
    private readonly IGenericRepository<CallRecord> _callRecordRepository;

    public StatisticService(
        ILogger<StatisticService> logger,
        IGenericRepository<CallRecord> callRecordRepository)
    {
        _logger = logger;
        _callRecordRepository = callRecordRepository;
    }

    /// <summary>
    /// Builds a query filter expression based on the provided statistics filter DTO.
    /// Uses the external PredicateBuilder static class for combining expressions.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>An expression tree for filtering CallRecord entities.</returns>
    private Expression<Func<CallRecord, bool>> BuildFilterExpression(StatisticsFilterDto filter)
    {
        Expression<Func<CallRecord, bool>> predicate = c => true;

        if (filter.StartDate.HasValue)
        {
            var startDateUtc = filter.StartDate.Value.ToUniversalTime();
            predicate = predicate.And(c => c.StartTime >= startDateUtc);
        }

        if (filter.EndDate.HasValue)
        {
            var endDateUtc = filter.EndDate.Value.ToUniversalTime();
            predicate = predicate.And(c => c.EndTime <= endDateUtc);
        }

        if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
        {
            var phoneNumber = filter.PhoneNumber.Trim().ToLower();
            Expression<Func<CallRecord, bool>> phonePredicate = c => c.CallerId.Trim().ToLower().Contains(phoneNumber) 
                                                                     || c.Recipient.Trim().ToLower().Contains(phoneNumber);
            predicate = predicate.And(phonePredicate);
        }

        if (!string.IsNullOrWhiteSpace(filter.Currency))
        {
            var currency = filter.Currency.Trim().ToLower();
            predicate = predicate.And(c => c.Currency.Trim().ToLower() == currency);
        }

        return predicate;
    }

    /// <summary>
    /// Calculates the average cost of call records based on the provided filter.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>The average call cost.</returns>
    public async Task<Result<decimal>> GetAverageCallCostAsync(StatisticsFilterDto filter)
    {
        try
        {
            var predicate = BuildFilterExpression(filter);
            var totalCost = await _callRecordRepository.SumAsync(predicate, c => c.Cost);
            var totalCount = await _callRecordRepository.CountAsync(predicate);

            if (totalCost.IsError)
            {
                return Error.Unexpected(code: "error_summing_call_costs");
            }
            if (totalCount.IsError)
            {
                return Error.Unexpected(code: "error_counting_calls_for_average_cost");
            }


            if (totalCount.Value == 0)
            {
                return 0m; 
            }

            return  Math.Round(totalCost.Value / totalCount.Value, 3, MidpointRounding.AwayFromZero);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average call cost with filter: {@filter}", filter);
            return Error.Unexpected("An error occurred while calculating average call cost.");
        }
    }

    /// <summary>
    /// Gets the total number of call records based on the provided filter.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>The total call count.</returns>
    public async Task<Result<int>> GetTotalCallCountAsync(StatisticsFilterDto filter)
    {
        try
        {
            var predicate = BuildFilterExpression(filter);
            var totalCount = await _callRecordRepository.CountAsync(predicate);
            return totalCount; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total call count with filter: {@filter}", filter);
            return Error.Unexpected("An error occurred while getting total call count.");
        }
    }

    /// <summary>
    /// Calculates the average duration of call records based on the provided filter.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>The average call duration.</returns>
    public async Task<Result<TimeSpan>> GetAverageCallDurationAsync(StatisticsFilterDto filter)
    {
        try
        {
            var predicate = BuildFilterExpression(filter);

            var averageDurationSecondsResult = await _callRecordRepository.AverageAsync(
                filter:predicate,
                selector: c => (decimal)((c.EndTime - c.StartTime).TotalSeconds));

            if (averageDurationSecondsResult.IsError)
            {
                return Error.Unexpected(code: "error_calculating_average_duration_in_repository");
            }

            if (averageDurationSecondsResult.Value == 0 && await _callRecordRepository.CountAsync(predicate) == 0)
            {
                return TimeSpan.Zero; 
            }

            return TimeSpan.FromSeconds((int)averageDurationSecondsResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average call duration with filter: {@filter}", filter);
            return Error.Unexpected("An error occurred while calculating average call duration.");
        }
    }

    /// <summary>
    /// Retrieves the top N longest call records based on the provided filter.
    /// </summary>
    /// <param name="count">The number of longest calls to return.</param>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>A list of the longest call records.</returns>
    public async Task<Result<IEnumerable<CallRecord>>> GetLongestCallsAsync(int count, StatisticsFilterDto filter)
    {
        try
        {
            if (count <= 0)
            {
                return new List<CallRecord>();
            }
            var predicate = BuildFilterExpression(filter);
            var longestCalls = await _callRecordRepository.GetListAsync<CallRecord>(
                predicate,
                orderBy: q => q.OrderByDescending(c => (c.EndTime - c.StartTime).TotalSeconds), 
                take: count);

            return longestCalls; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting longest calls with filter: {@filter} and count: {count}", filter, count);
            return Error.Unexpected("An error occurred while getting longest calls.");
        }
    }

    /// <summary>
    /// Calculates the average number of calls within a specified time granularity over a given date range.
    /// </summary>
    /// <param name="filter">The filter criteria including date range and granularity.</param>
    /// <returns>The average number of calls per period.</returns>
    public async Task<Result<double>> GetCallsPerPeriodAsync(StatisticsPerPeriodFilterDto filter)
    {
        try
        {
            if (!filter.StartDate.HasValue || !filter.EndDate.HasValue)
            {
                return Error.Validation(code: "date_are_required_for_calls_per_period_calculation");
            }

            var predicate = BuildFilterExpression(filter);
            var totalCallsResult = await _callRecordRepository.CountAsync(predicate);

            if (totalCallsResult.IsError)
            {
                return Error.Unexpected(code: "error_getting_total_call_count_for_calls_per_period");
            }
            var totalCalls = totalCallsResult.Value;

            if (totalCalls == 0)
            {
                return 0.0;
            }

            double totalPeriods = 0;
            var startDateUtc = filter.StartDate.Value.ToUniversalTime();
            var endDateUtc = filter.EndDate.Value.ToUniversalTime();
            var timeSpan = endDateUtc - startDateUtc;

            switch (filter.Granularity)
            {
                case StatisticsGranularity.Hourly:
                    totalPeriods = timeSpan.TotalHours;
                    break;
                case StatisticsGranularity.Daily:
                    totalPeriods = timeSpan.TotalDays;
                    break;
                case StatisticsGranularity.Monthly:
                    totalPeriods = (endDateUtc.Year - startDateUtc.Year) * 12 + endDateUtc.Month - startDateUtc.Month;
                    if (endDateUtc.Day < startDateUtc.Day && totalPeriods > 0)
                        totalPeriods--; 
                    if (totalPeriods <= 0)
                        totalPeriods = 1; 
                    break;
                case StatisticsGranularity.Yearly:
                    totalPeriods = endDateUtc.Year - startDateUtc.Year;
                    if (endDateUtc.DayOfYear < startDateUtc.DayOfYear && totalPeriods > 0)
                        totalPeriods--; 
                    if (totalPeriods <= 0)
                        totalPeriods = 1; 
                    break;
            }

            if (totalPeriods <= 0) 
            {
                totalPeriods = 1;
            }

            return totalCalls / totalPeriods;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating calls per period with filter: {@filter}", filter);
            return Error.Unexpected("An error occurred while calculating calls per period.");
        }
    }

    /// <summary>
    /// Provides data points showing the number of calls over a specific time period, grouped by a specified granularity.
    /// </summary>
    /// <param name="filter">The filter criteria including date range and granularity.</param>
    /// <returns>A list of data points representing the call volume trend.</returns>
    public async Task<Result<IEnumerable<CallVolumeDataPointDto>>> GetCallVolumeTrendAsync(StatisticsVolumeTrendFilterDto filter)
    {
        try
        {
            if (!filter.StartDate.HasValue || !filter.EndDate.HasValue)
            {
                return Error.Validation(code: "date_are_required_for_call_volume_trend");
            }

            var predicate = BuildFilterExpression(filter);
            
            var allCallsInDateRangeResult = await _callRecordRepository.GetListAsync<CallRecord>(predicate);

            if (allCallsInDateRangeResult.IsError)
            {
                return Error.Unexpected(code:"error_fetching_calls_for_volume_trend");
            }
            var allCallsInDateRange = allCallsInDateRangeResult.Value;

            if (!allCallsInDateRange.Any())
            {
                return new List<CallVolumeDataPointDto>();
            }

            var groupedCalls = allCallsInDateRange
                .GroupBy(c => GetPeriodStart(c.StartTime, filter.Granularity))
                .Select(g => new CallVolumeDataPointDto
                {
                    Period = g.Key,
                    CallCount = g.Count()
                })
                .OrderBy(dp => dp.Period)
                .ToList();

            return groupedCalls;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting call volume trend with filter: {@filter}", filter);
            return Error.Unexpected("An error occurred while getting call volume trend.");
        }
    }

    /// <summary>
    /// Helper method to get the start of a time period based on granularity.
    /// Note: This logic is simplified for in-memory grouping. Database-specific
    /// date truncation functions are usually more efficient.
    /// </summary>
    private DateTimeOffset GetPeriodStart(DateTimeOffset dateTime, StatisticsGranularity granularity)
    {
        var utcDateTime = dateTime.ToUniversalTime(); 

        return granularity switch
        {
            StatisticsGranularity.Hourly => new DateTimeOffset(utcDateTime.Year, utcDateTime.Month, utcDateTime.Day, utcDateTime.Hour, 0, 0, TimeSpan.Zero),
            StatisticsGranularity.Daily => new DateTimeOffset(utcDateTime.Year, utcDateTime.Month, utcDateTime.Day, 0, 0, 0, TimeSpan.Zero),
            StatisticsGranularity.Weekly => GetStartOfWeek(utcDateTime),
            StatisticsGranularity.Monthly => new DateTimeOffset(utcDateTime.Year, utcDateTime.Month, 1, 0, 0, 0, TimeSpan.Zero),
            StatisticsGranularity.Yearly => new DateTimeOffset(utcDateTime.Year, 1, 1, 0, 0, 0, TimeSpan.Zero),
            _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, null)
        };
    }

    /// <summary>
    /// Helper to get the start of the week (assuming Monday as the start).
    /// </summary>
    private DateTimeOffset GetStartOfWeek(DateTimeOffset dateTime)
    {
        var utcDateTime = dateTime.ToUniversalTime();
        int diff = (7 + (utcDateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeek = utcDateTime.AddDays(-1 * diff).Date;
         return new DateTimeOffset(startOfWeek, TimeSpan.Zero);
    }


    /// <summary>
    /// Calculates the total cost of calls, grouped by currency, based on the provided filter.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <returns>A dictionary where keys are currency codes and values are the total costs.</returns>
    public async Task<Result<Dictionary<string, decimal>>> GetTotalCostByCurrencyAsync(StatisticsFilterDto filter)
    {
        try
        {
            var predicate = BuildFilterExpression(filter);

            var allCallsInDateRangeResult = await _callRecordRepository.GetListAsync<CallRecord>(predicate);

            if (allCallsInDateRangeResult.IsError)
            {
                return Error.Unexpected(code: "error_fetching_calls_for_cost_by_currency");
            }
            var allCallsInDateRange = allCallsInDateRangeResult.Value;

            if (!allCallsInDateRange.Any())
            {
                return new Dictionary<string, decimal>();
            }

            var totalCostByCurrency = allCallsInDateRange
                .GroupBy(c => c.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Cost));


            return totalCostByCurrency;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total cost by currency with filter: {@filter}", filter);
            return Error.Unexpected("An error occurred while calculating total cost by currency.");
        }
    }
}
