namespace CallRecordIntelligence.API.Services.Interfaces;

public interface ICallRecordService
{
    Task<Result<CallRecord>> GetCallRecordAsync(Guid callRecordId);
    Task<Result<CallRecord>> GetCallRecordAsync(string reference);
    Task<Result<PaginationResponse<CallRecord>>> GetCallRecordsAsync(
        int page = 0, 
        int pageSize = 50,
        string? phoneNumber = null, 
        DateTimeOffset? startTimestamp = null, 
        DateTimeOffset? endTimestamp = null);
     
    Task<Result<CallRecord>> AddCallRecordAsync(AddCallRecordRequest request);
    Task<Result<bool>> AddCallRecordsRangeAsync(IEnumerable<AddCallRecordRequest> requests);
    
    Task<Result<CallRecord>> UpdateCallRecordAsync(Guid callRecordId, UpdateCallRecordRequest request);
    Task<Result<CallRecord>> UpdateCallRecordAsync(string reference, UpdateCallRecordRequest request);
    
    Task<Result<CallRecord>> RemoveCallRecordAsync(Guid callRecordId);
    Task<Result<CallRecord>> RemoveCallRecordAsync(string reference);

}