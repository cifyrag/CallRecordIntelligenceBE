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
    
    public async Task<Result<CallRecord>> GetCallRecordAsync(Guid callRecordId)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while getting call record by id: {callRecordId}",
                callRecordId);

            return Error.Unexpected();
        }
    }

    public async Task<Result<CallRecord>> GetCallRecordAsync(string reference)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while getting call record by reference: {reference}",
                reference);

            return Error.Unexpected();
        }
    }

    public async Task<Result<IEnumerable<CallRecord>>> GetCallRecordsAsync(string? phoneNumber = null, DateTimeOffset? startTimestamp = null,
        DateTimeOffset? endTimestamp = null)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while getting call records list");

            return Error.Unexpected();
        }
    }

    #endregion
    
    #region POST
    
    public async Task<Result<CallRecord>> AddCallRecordAsync(AddCallRecordRequest request)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while adding call record");

            return Error.Unexpected();
        }
    }

    public async Task<Result<List<CallRecord>>> AddCallRecordsRangeAsync(List<AddCallRecordRequest> request)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while adding call records list");

            return Error.Unexpected();
        }
    }

    #endregion
    
    #region PUT
    
    public async Task<Result<CallRecord>> UpdateCallRecordAsync(Guid callRecordId, AddCallRecordRequest request)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while updating call record by id: {callRecordId}",
                callRecordId);

            return Error.Unexpected();
        }
    }

    public async Task<Result<CallRecord>> UpdateCallRecordAsync(string reference, AddCallRecordRequest request)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while updating call record by reference: {reference}",
                reference);

            return Error.Unexpected();
        }
    }
    
    #endregion
    
    #region DELETE
    
    public async Task<Result<CallRecord>> RemoveCallRecordAsync(Guid callRecordId)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while deleting call record by id: {callRecordId}",
                callRecordId);

            return Error.Unexpected();
        }
    }

    public async Task<Result<CallRecord>> RemoveCallRecordAsync(string reference)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while deleting call record by reference: {reference}",
                reference);

            return Error.Unexpected();
        }
    }
    
    #endregion
    
}