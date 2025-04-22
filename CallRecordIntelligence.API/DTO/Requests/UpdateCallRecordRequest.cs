using System.ComponentModel.DataAnnotations;

namespace CallRecordIntelligence.API.DTO.Requests;

public class UpdateCallRecordRequest
{
    [MaxLength(20, ErrorMessage = "Caller ID cannot exceed 20 characters.")]
    public string? CallerId { get; set; }= null;

    [MaxLength(20, ErrorMessage = "Recipient cannot exceed 20 characters.")]
    public string? Recipient { get; set; }= null;

    public DateTimeOffset? StartTime { get; set; }= null;

    public DateTimeOffset? EndTime { get; set; }= null;

    [Range(0.001, double.MaxValue, ErrorMessage = "Cost must be greater than 0.")]
    public decimal? Cost { get; set; }= null;

    public string? Reference { get; set; }= null;

    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter code.")]
    public string? Currency { get; set; } = null;
}