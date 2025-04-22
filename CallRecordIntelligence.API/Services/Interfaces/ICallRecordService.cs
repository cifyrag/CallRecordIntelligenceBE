namespace CallRecordIntelligence.API.Services.Interfaces;

public interface ICallRecordService
{
    Task<Result<CallRecord>> GetCallRecordAsync(Guid callRecordId);
    Task<Result<CallRecord>> GetCallRecordAsync(string reference);
    Task<Result<IEnumerable<CallRecord>>> GetCallRecordsAsync(
        string? phoneNumber = null, 
        DateTimeOffset? startTimestamp = null, 
        DateTimeOffset? endTimestamp = null);
    
    
    Task<Result<CallRecord>> AddCallRecordAsync(AddCallRecordRequest request);
    Task<Result<List<CallRecord>>> AddCallRecordsRangeAsync(List<AddCallRecordRequest> request);
    
    Task<Result<CallRecord>> UpdateCallRecordAsync(Guid callRecordId, AddCallRecordRequest request);
    Task<Result<CallRecord>> UpdateCallRecordAsync(string reference, AddCallRecordRequest request);
    
    Task<Result<CallRecord>> RemoveCallRecordAsync(Guid callRecordId);
    Task<Result<CallRecord>> RemoveCallRecordAsync(string reference);

}