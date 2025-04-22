namespace CallRecordIntelligence.API.DTO.Responses;

public class CallRecordDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CallerId { get; set; }
    public string Recipient { get; set; }
    public DateOnly CallDate { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public int Duration { get; set; }
    public decimal Cost { get; set; }
    public string Reference { get; set; }
    public string Currency { get; set; }

}

public static class CallRecordExtensions
{
    public static CallRecordDto ToCallRecordDto(this CallRecord callRecord)
        => new CallRecordDto
        {
            Id = callRecord.Id,
            CallerId = callRecord.CallerId,
            Recipient = callRecord.Recipient,
            StartTime = callRecord.StartTime,
            EndTime = callRecord.EndTime,
            Cost = callRecord.Cost,
            Reference = callRecord.Reference,
            Currency = callRecord.Currency,
            Duration = callRecord.Duration,
            CallDate = callRecord.CallDate,
        };
}