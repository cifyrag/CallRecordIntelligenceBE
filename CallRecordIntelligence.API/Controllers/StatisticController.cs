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
    
    #endregion
    
    #region POST
    
    #endregion
    
    #region PUT
    
    #endregion
    
    #region DELETE
    
    #endregion
}