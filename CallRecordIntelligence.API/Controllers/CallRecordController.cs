namespace CallRecordIntelligence.API.Controllers;

[Route("/call-record-api/v1/")]
[ApiController]
public class CallRecordController: ControllerBase
{
    private readonly ICallRecordService _callRecordService;

    public CallRecordController(
        ICallRecordService callRecordService)
    {
        _callRecordService = callRecordService;
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