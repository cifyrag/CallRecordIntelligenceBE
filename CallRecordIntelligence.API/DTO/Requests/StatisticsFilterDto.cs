using System.ComponentModel.DataAnnotations;

namespace CallRecordIntelligence.API.DTO.Requests;

public class StatisticsFilterDto
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Currency { get; set; }
}

public class StatisticsPerPeriodFilterDto : StatisticsFilterDto
{
    [Required]
    public StatisticsGranularity Granularity { get; set; } 
}

public class StatisticsVolumeTrendFilterDto : StatisticsFilterDto
{
    [Required]
    public StatisticsGranularity Granularity { get; set; }
}

public class CallVolumeDataPointDto
{
    public DateTimeOffset Period { get; set; } 
    public int CallCount { get; set; }
}
