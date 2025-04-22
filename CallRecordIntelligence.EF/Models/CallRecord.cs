using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallRecordIntelligence.EF.Models;

public class CallRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("caller_id", TypeName = "VARCHAR")]
    [MaxLength(20)]
    public string CallerId { get; set; }

    [Required]
    [Column("recipient", TypeName = "VARCHAR")]
    [MaxLength(20)]
    public string Recipient { get; set; }

    [Required]
    [Column("call_date", TypeName = "DATE")]
    public DateOnly CallDate
    {
        get => DateOnly.FromDateTime(StartTime.DateTime);
    }

    [Required]
    [Column("call_start", TypeName = "TIMESTAMP WITH TIME ZONE")]
    public DateTimeOffset StartTime { get; set; }

    [Required]
    [Column("end_time", TypeName = "TIMESTAMP WITH TIME ZONE")]
    public DateTimeOffset EndTime { get; set; }

    [Required]
    public int Duration 
    {
        get => Convert.ToInt32((EndTime - StartTime).TotalSeconds);
    }

    [Required]
    [Column("cost", TypeName = "DECIMAL(10, 3)")]
    public decimal Cost { get; set; }

    [Required]
    [Column("reference", TypeName = "VARCHAR")]
    public string Reference { get; set; }

    [Required]
    [Column("currency", TypeName = "VARCHAR")]
    [StringLength(3)]
    public string Currency { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset Inserted { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTimeOffset LastUpdated { get; set; }
}