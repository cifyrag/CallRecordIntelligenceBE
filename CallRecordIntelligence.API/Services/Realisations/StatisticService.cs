namespace CallRecordIntelligence.API.Services.Realisations;

public class StatisticService: IStatisticService
{
    private readonly ILogger<StatisticService> _logger;
    private readonly IGenericRepository<CallRecordService> _callRecordService;

    public StatisticService(
        ILogger<StatisticService> logger,
        IGenericRepository<CallRecordService> callRecordService)
    {
        _logger = logger;
        _callRecordService = callRecordService;
    }
    
    #region GET
    
    #endregion
    
    #region POST
    
    #endregion
    
    #region PUT
    
    #endregion
    
    #region DELETE
    
    #endregion
}