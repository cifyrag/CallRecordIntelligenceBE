using System.ComponentModel.DataAnnotations;

namespace CallRecordIntelligence.API.DTO.Requests;

public class AddCallRecordRequest
{
    [Required(ErrorMessage = "Caller ID is required.")]
    [MaxLength(20, ErrorMessage = "Caller ID cannot exceed 20 characters.")]
    public string CallerId { get; set; }

    [Required(ErrorMessage = "Recipient is required.")]
    [MaxLength(20, ErrorMessage = "Recipient cannot exceed 20 characters.")]
    public string Recipient { get; set; }

    [Required(ErrorMessage = "Call start time is required.")]
    public DateTimeOffset StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    public DateTimeOffset EndTime { get; set; }

    [Required(ErrorMessage = "Cost is required.")]
    [Range(0.001, double.MaxValue, ErrorMessage = "Cost must be greater than 0.")]
    public decimal Cost { get; set; }

    [Required(ErrorMessage = "Reference is required.")]
    public string Reference { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter code.")]
    public string Currency { get; set; }
}