using CallRecordIntelligence.EF.Models;

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
    
    public Task<Result<CallRecord>> GetCallRecordAsync(Guid callRecordId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CallRecord>> GetCallRecordAsync(string reference)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<CallRecord>>> GetCallRecordsAsync(string? phoneNumber = null, DateTimeOffset? startTimestamp = null,
        DateTimeOffset? endTimestamp = null)
    {
        throw new NotImplementedException();
    }

    #endregion
    
    #region POST
    
    public Task<Result<CallRecord>> AddCallRecordAsync(AddCallRecordRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Result<List<CallRecord>>> AddCallRecordsRangeAsync(List<AddCallRecordRequest> request)
    {
        throw new NotImplementedException();
    }

    #endregion
    
    #region PUT
    
    public Task<Result<CallRecord>> UpdateCallRecordAsync(Guid callRecordId, AddCallRecordRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CallRecord>> UpdateCallRecordAsync(string reference, AddCallRecordRequest request)
    {
        throw new NotImplementedException();
    }
    
    #endregion
    
    #region DELETE
    
    public Task<Result<CallRecord>> RemoveCallRecordAsync(Guid callRecordId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CallRecord>> RemoveCallRecordAsync(string reference)
    {
        throw new NotImplementedException();
    }
    
    #endregion
    
}