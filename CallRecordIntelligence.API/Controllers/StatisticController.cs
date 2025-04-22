namespace CallRecordIntelligence.API.Controllers;

[Route("/statistic-api/v1/")]
[ApiController]
public class StatisticController: ControllerBase
{
    private readonly IStatisticService _statisticService;
    
    public StatisticController(
        IStatisticService statisticService)
    {
        _statisticService = statisticService;
    }
    
    #region GET

    /// <summary>
    /// Retrieves the average cost of call records based on the provided filter.
    /// </summary>
    /// <param name="filter">The filter criteria (optional query parameters).</param>
    /// <returns>The average call cost.</returns>
    /// <response code="200">Returns the average call cost.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("average-cost")]
    [ProducesResponseType(typeof(decimal), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAverageCallCostAsync([FromQuery] StatisticsFilterDto filter)
    {
        var result = await _statisticService.GetAverageCallCostAsync(filter);

        if (result.IsError)
        {
            return BadRequest(result.Error); 
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets the total number of call records based on the provided filter.
    /// </summary>
    /// <param name="filter">The filter criteria (optional query parameters).</param>
    /// <returns>The total call count.</returns>
    /// <response code="200">Returns the total call count.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("total-calls")]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetTotalCallCountAsync([FromQuery] StatisticsFilterDto filter)
    {
        var result = await _statisticService.GetTotalCallCountAsync(filter);

        if (result.IsError)
        {
            return BadRequest(result.Error); 
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Calculates the average duration of call records based on the provided filter.
    /// </summary>
    /// <param name="filter">The filter criteria (optional query parameters).</param>
    /// <returns>The average call duration.</returns>
    /// <response code="200">Returns the average call duration (as a TimeSpan or string representation).</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("average-duration")]
    [ProducesResponseType(typeof(TimeSpan), 200)] 
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAverageCallDurationAsync([FromQuery] StatisticsFilterDto filter)
    {
        var result = await _statisticService.GetAverageCallDurationAsync(filter);

        if (result.IsError)
        {
             return BadRequest(result.Error); 
        }

        return Ok(result.Value); 
    }

    /// <summary>
    /// Retrieves the top N longest call records based on the provided filter.
    /// </summary>
    /// <param name="count">The number of longest calls to return.</param>
    /// <param name="filter">The filter criteria (optional query parameters).</param>
    /// <returns>A list of the longest call records.</returns>
    /// <response code="200">Returns a list of call records.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("longest-calls/{count:int}")]
    [ProducesResponseType(typeof(IEnumerable<CallRecordDto>), 200)] 
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetLongestCallsAsync([FromRoute] int count, [FromQuery] StatisticsFilterDto filter)
    {
        var result = await _statisticService.GetLongestCallsAsync(count, filter);

        if (result.IsError)
        {
             return BadRequest(result.Error); 
        }

        return Ok(result.Value.Select(o=>o.ToCallRecordDto())); 
    }

    /// <summary>
    /// Calculates the average number of calls within a specified time granularity over a given date range.
    /// </summary>
    /// <param name="filter">The filter criteria including date range and granularity (query parameters).</param>
    /// <returns>The average number of calls per period.</returns>
    /// <response code="200">Returns the average number of calls per period.</response>
    /// <response code="400">If the request parameters are invalid (e.g., missing dates, invalid granularity).</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("calls-per-period")]
    [ProducesResponseType(typeof(double), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCallsPerPeriodAsync([FromQuery] StatisticsPerPeriodFilterDto filter)
    {
        var result = await _statisticService.GetCallsPerPeriodAsync(filter);

        if (result.IsError)
        {
            if (result.Error.Code == "validation")
            {
                return BadRequest(result.Error);
            }
            
            return BadRequest(result.Error); 
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Provides data points showing the number of calls over a specific time period, grouped by a specified granularity.
    /// </summary>
    /// <param name="filter">The filter criteria including date range and granularity (query parameters).</param>
    /// <returns>A list of data points representing the call volume trend.</returns>
    /// <response code="200">Returns a list of call volume data points.</response>
    /// <response code="400">If the request parameters are invalid (e.g., missing dates, invalid granularity).</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("call-volume-trend")]
    [ProducesResponseType(typeof(IEnumerable<CallVolumeDataPointDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCallVolumeTrendAsync([FromQuery] StatisticsVolumeTrendFilterDto filter)
    {
        var result = await _statisticService.GetCallVolumeTrendAsync(filter);

        if (result.IsError)
        {
            if (result.Error.Code == "validation")
            {
                return BadRequest(result.Error);
            }
             return BadRequest(result.Error); 
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Calculates the total cost of calls, grouped by currency, based on the provided filter.
    /// </summary>
    /// <param name="filter">The filter criteria (optional query parameters).</param>
    /// <returns>A dictionary where keys are currency codes and values are the total costs.</returns>
    /// <response code="200">Returns a dictionary of total costs by currency.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("cost-by-currency")]
    [ProducesResponseType(typeof(Dictionary<string, decimal>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetTotalCostByCurrencyAsync([FromQuery] StatisticsFilterDto filter)
    {
        var result = await _statisticService.GetTotalCostByCurrencyAsync(filter);

        if (result.IsError)
        {
             return BadRequest(result.Error); 
        }

        return Ok(result.Value);
    }

    #endregion
}