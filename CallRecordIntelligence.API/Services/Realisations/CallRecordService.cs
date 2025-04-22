namespace CallRecordIntelligence.API.Services.Realisations;

public class CallRecordService: ICallRecordService
{
    private readonly ILogger<CallRecordService> _logger;
    private readonly IGenericRepository<CallRecord> _callRecordRepository;

    public CallRecordService(
        ILogger<CallRecordService> logger, IGenericRepository<CallRecord> callRecordRepository)
    {
        _logger = logger;
        _callRecordRepository = callRecordRepository;
    }
    
    #region GET
    
    public async Task<Result<CallRecord>> GetCallRecordAsync(Guid callRecordId)
    {
        try
        {
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Id == callRecordId);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }

            return callRecord;
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
            if (string.IsNullOrWhiteSpace(reference))
            {
                return Error.Validation(code: "reference_is_required");
            }
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Reference == reference);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }

            return callRecord;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while getting call record by reference: {reference}",
                reference);

            return Error.Unexpected();
        }
    }

    public async Task<Result<PaginationResponse<CallRecord>>> GetCallRecordsAsync(
        int page = 0, 
        int pageSize = 50,
        string? phoneNumber = null, 
        DateTimeOffset? startTimestamp = null, 
        DateTimeOffset? endTimestamp = null)
    {
        try
        {
            var total = await _callRecordRepository.CountAsync(
                filter: c => 
                    (phoneNumber == null 
                     || (c.Recipient.Contains(phoneNumber) 
                         || c.CallerId.Contains(phoneNumber))) 
                    && (startTimestamp == null 
                        || c.StartTime >= startTimestamp) 
                    && (endTimestamp == null 
                        || c.EndTime <= endTimestamp));
            
            Result<IEnumerable<CallRecord>> callRecords = new List<CallRecord>();

            if (total.Value > 0)
            {
                callRecords = await _callRecordRepository.GetListAsync<CallRecord>(
                    filter: c =>
                        (phoneNumber == null
                         || (c.Recipient.Contains(phoneNumber)
                             || c.CallerId.Contains(phoneNumber)))
                        && (startTimestamp == null
                            || c.StartTime >= startTimestamp)
                        && (endTimestamp == null
                            || c.EndTime <= endTimestamp),
                    skip: page * pageSize,
                    take: pageSize);
            }
            
            if (total.Value == 0 || callRecords?.Value is null || !callRecords.Value.Any())
            {
                return new List<CallRecord>().ToPageResponse(page, pageSize, total.Value);
            }

            return callRecords.Value.ToList()
                .ToPageResponse(page, pageSize, total.Value);
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
            var newCallRecord = new CallRecord
            {
                CallerId = request.CallerId,
                Recipient = request.Recipient,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Cost = request.Cost,
                Reference = request.Reference,
                Currency = request.Currency
            };
            
            return await _callRecordRepository.AddAsync(newCallRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while adding call record");

            return Error.Unexpected();
        }
    }

    public async Task<Result<bool>> AddCallRecordsRangeAsync(IEnumerable<AddCallRecordRequest> requests)
    {
        try
        {
            var callrecords = requests.Select(request => new CallRecord
            {
                CallerId = request.CallerId,
                Recipient = request.Recipient,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Cost = request.Cost,
                Reference = request.Reference,
                Currency = request.Currency
            });
            
            return await _callRecordRepository.AddRangeAsync(callrecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while adding call records list");

            return Error.Unexpected();
        }
    }

    #endregion
    
    #region PUT
    
    public async Task<Result<CallRecord>> UpdateCallRecordAsync(Guid callRecordId, UpdateCallRecordRequest request)
    {
        try
        {
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Id == callRecordId);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }
            
            callRecord.Value.CallerId = request.CallerId ?? callRecord.Value.CallerId;
            callRecord.Value.Recipient = request.Recipient ?? callRecord.Value.Recipient;
            callRecord.Value.StartTime = request.StartTime ?? callRecord.Value.StartTime;
            callRecord.Value.EndTime = request.EndTime ?? callRecord.Value.EndTime;
            callRecord.Value.Cost = request.Cost ?? callRecord.Value.Cost;
            callRecord.Value.Reference = request.Reference ?? callRecord.Value.Reference;
            callRecord.Value.Currency = request.Currency ?? callRecord.Value.Currency;
            
            return await _callRecordRepository.UpdateAsync(callRecord.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while updating call record by id: {callRecordId}",
                callRecordId);

            return Error.Unexpected();
        }
    }

    public async Task<Result<CallRecord>> UpdateCallRecordAsync(string reference, UpdateCallRecordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return Error.Validation(code: "reference_is_required");
            }
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Reference == reference);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }
            
            callRecord.Value.CallerId = request.CallerId ?? callRecord.Value.CallerId;
            callRecord.Value.Recipient = request.Recipient ?? callRecord.Value.Recipient;
            callRecord.Value.StartTime = request.StartTime ?? callRecord.Value.StartTime;
            callRecord.Value.EndTime = request.EndTime ?? callRecord.Value.EndTime;
            callRecord.Value.Cost = request.Cost ?? callRecord.Value.Cost;
            callRecord.Value.Reference = request.Reference ?? callRecord.Value.Reference;
            callRecord.Value.Currency = request.Currency ?? callRecord.Value.Currency;
            
            return await _callRecordRepository.UpdateAsync(callRecord.Value);
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
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Id == callRecordId);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }
            
            return await _callRecordRepository.RemoveAsync(callRecord.Value);
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
            if (string.IsNullOrWhiteSpace(reference))
            {
                return Error.Validation(code: "reference_is_required");
            }
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Reference == reference);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }
            
            return await _callRecordRepository.RemoveAsync(callRecord.Value);
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