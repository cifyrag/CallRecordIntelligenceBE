namespace CallRecordIntelligence.API.Services.Realisations;

public class CallRecordService: ICallRecordService
{
    private readonly ILogger<CallRecordService> _logger;
    private readonly IGenericRepository<CallRecordService> _callRecordService;

    public CallRecordService(
        ILogger<CallRecordService> logger,
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