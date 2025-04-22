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
    Task<Result<PaginationResponse<CallRecord>>> AddCallRecordsRangeAsync(
        IEnumerable<AddCallRecordRequest> request,
        int page = 0, 
        int pageSize = 50);
    
    Task<Result<CallRecord>> UpdateCallRecordAsync(Guid callRecordId, AddCallRecordRequest request);
    Task<Result<CallRecord>> UpdateCallRecordAsync(string reference, AddCallRecordRequest request);
    
    Task<Result<CallRecord>> RemoveCallRecordAsync(Guid callRecordId);
    Task<Result<CallRecord>> RemoveCallRecordAsync(string reference);

}