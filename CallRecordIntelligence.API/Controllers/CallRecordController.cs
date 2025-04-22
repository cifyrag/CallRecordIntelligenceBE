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

   /// <summary>
   /// Retrieves details of a specific call record by its unique identifier.
   /// </summary>
   /// <param name="callRecordId">The unique identifier of the call record.</param>
   /// <returns>The call record details.</returns>
   /// <response code="200">Returns the call record details.</response>
   /// <response code="400">If the request parameters are invalid.</response>
   /// <response code="404">If the call record with the specified ID is not found.</response>
   /// <response code="500">If an internal server error occurs.</response>
   [HttpGet("{callRecordId:guid}")]
   [ProducesResponseType(typeof(CallRecordDto), 200)]
   [ProducesResponseType(400)]
   [ProducesResponseType(404)] 
   [ProducesResponseType(500)]
   public async Task<IActionResult> GetCallRecordByIdAsync([FromRoute] Guid callRecordId)
   {
       var result = await _callRecordService.GetCallRecordAsync(callRecordId);

       if (result.IsError)
       {
           if (result.Error.Code == "call_record_not_found")
           {
               return NotFound(result.Error);
           }
           return BadRequest(result.Error);
       }

       return Ok(result.Value.ToCallRecordDto());
   }

   /// <summary>
   /// Retrieves details of a specific call record by its reference string.
   /// </summary>
   /// <param name="reference">The unique reference string of the call record.</param>
   /// <returns>The call record details.</returns>
   /// <response code="200">Returns the call record details.</response>
   /// <response code="400">If the request parameters are invalid.</response>
   /// <response code="404">If the call record with the specified reference is not found.</response>
   /// <response code="500">If an internal server error occurs.</response>
   [HttpGet("reference/{reference}")]
   [ProducesResponseType(typeof(CallRecordDto), 200)]
   [ProducesResponseType(400)]
   [ProducesResponseType(404)] 
   [ProducesResponseType(500)]
   public async Task<IActionResult> GetCallRecordByReferenceAsync([FromRoute] string reference)
   {
       var result = await _callRecordService.GetCallRecordAsync(reference);

       if (result.IsError)
       {
           if (result.Error.Code == "call_record_not_found")
           {
               return NotFound(result.Error);
           }
           return BadRequest(result.Error);
       }

       return Ok(result.Value.ToCallRecordDto());
   }

   /// <summary>
   /// Retrieves a paginated list of call records with optional filtering.
   /// </summary>
   /// <param name="page">The page number (0-based) for pagination. Defaults to 0.</param>
   /// <param name="pageSize">The number of records per page for pagination. Defaults to 50.</param>
   /// <param name="phoneNumber">Optional phone number to filter by caller or recipient.</param>
   /// <param name="startTimestamp">Optional start timestamp to filter by call start time.</param>
   /// <param name="endTimestamp">Optional end timestamp to filter by call end time.</param>
   /// <returns>A paginated list of call records.</returns>
   /// <response code="200">Returns the paginated list of call records.</response>
   /// <response code="400">If the request parameters are invalid.</response>
   /// <response code="500">If an internal server error occurs.</response>
   [HttpGet]
   [ProducesResponseType(typeof(PaginationResponse<CallRecordDto>), 200)]
   [ProducesResponseType(400)]
   [ProducesResponseType(500)]
   public async Task<IActionResult> GetCallRecordsAsync(
       [FromQuery] int page = 0,
       [FromQuery] int pageSize = 50,
       [FromQuery] string? phoneNumber = null,
       [FromQuery] DateTimeOffset? startTimestamp = null,
       [FromQuery] DateTimeOffset? endTimestamp = null)
   {
       var result = await _callRecordService.GetCallRecordsAsync(
           page: page,
           pageSize: pageSize,
           phoneNumber: phoneNumber,
           startTimestamp: startTimestamp,
           endTimestamp: endTimestamp);

       if (result.IsError)
       {
           return BadRequest(result.Error);
       }

       var responseDto = new PaginationResponse<CallRecordDto>
       (
           Items: result.Value.Items.Select(cr => cr.ToCallRecordDto()).ToList(), 
           NextPage: result.Value.NextPage,
           TotalPages: result.Value.TotalPages,
           Total: result.Value.Total 
       );

       return Ok(responseDto);
   }

   #endregion

   #region POST

   /// <summary>
   /// Uploads a CSV file containing call records and adds them in bulk.
   /// Assumes the CSV file has a header row and columns in the order:
   /// CallerId,Recipient,StartTime,EndTime,Cost,Reference,Currency
   /// StartTime and EndTime should be in a format parsable by DateTimeOffset.Parse.
   /// Cost should be in a format parsable by decimal.Parse.
   /// </summary>
   /// <param name="file">The CSV file to upload.</param>
   /// <returns>A result indicating the success or failure of the bulk upload.</returns>
   /// <response code="200">Call records from CSV added successfully.</response>
   /// <response code="400">If no file is uploaded, the file is empty, the file format is incorrect, or data parsing fails.</response>
   /// <response code="500">If an internal server error occurs during processing or adding records.</response>
   [HttpPost("upload-csv")]
   [ProducesResponseType(typeof(bool), 200)]
   [ProducesResponseType(400)]
   [ProducesResponseType(500)]
   public async Task<IActionResult> UploadCsvAsync(IFormFile file)
   {
       if (file == null || file.Length == 0)
       {
           return BadRequest("No file uploaded or file is empty.");
       }

       var result = await _callRecordService.AddCallRecordsFromCsvAsync(file.OpenReadStream());

       if (result.IsError)
       {
           return BadRequest(result.Error);
       }

       return Ok(result.Value);
   }
   
   /// <summary>
   /// Creates a new call record.
   /// </summary>
   /// <param name="request">The call record details to create.</param>
   /// <returns>The created call record details.</returns>
   /// <response code="200">Call record created successfully.</response>
   /// <response code="400">If the request parameters are invalid.</response>
   /// <response code="500">If an internal server error occurs.</response>
   [HttpPost]
   [ProducesResponseType(typeof(CallRecordDto), 200)]
   [ProducesResponseType(400)]
   [ProducesResponseType(500)]
   public async Task<IActionResult> AddCallRecordAsync([FromBody] AddCallRecordRequest request)
   {
       var result = await _callRecordService.AddCallRecordAsync(request);

       if (result.IsError)
       {
           return BadRequest(result.Error);
       }

       return Ok(result.Value.ToCallRecordDto());
   }

   /// <summary>
   /// Creates multiple new call records in a batch.
   /// </summary>
   /// <param name="requests">The list of call record details to create.</param>
   /// <returns>A boolean indicating success.</returns>
   /// <response code="200">Call records added successfully.</response>
   /// <response code="400">If the request parameters are invalid.</response>
   /// <response code="500">If an internal server error occurs.</response>
   [HttpPost("bulk")]
   [ProducesResponseType(typeof(bool), 200)]
   [ProducesResponseType(400)]
   [ProducesResponseType(500)]
   public async Task<IActionResult> AddCallRecordsRangeAsync([FromBody] IEnumerable<AddCallRecordRequest> requests)
   {
       var result = await _callRecordService.AddCallRecordsRangeAsync(requests);

       if (result.IsError)
       {
           return BadRequest(result.Error);
       }

       return Ok(result.Value);
   }

   #endregion

   #region PUT

   /// <summary>
   /// Updates an existing call record by its unique identifier.
   /// </summary>
   /// <param name="callRecordId">The unique identifier of the call record to update.</param>
   /// <param name="request">The updated call record details.</param>
   /// <returns>The updated call record details.</returns>
   /// <response code="200">Call record updated successfully.</response>
   /// <response code="400">If the request parameters are invalid.</response>
   /// <response code="404">If the call record with the specified ID is not found.</response>
   /// <response code="500">If an internal server error occurs.</response>
   [HttpPut("{callRecordId:guid}")]
   [ProducesResponseType(typeof(CallRecordDto), 200)]
   [ProducesResponseType(400)]
   [ProducesResponseType(404)] 
   [ProducesResponseType(500)]
   public async Task<IActionResult> UpdateCallRecordByIdAsync([FromRoute] Guid callRecordId, [FromBody] UpdateCallRecordRequest request)
   {
       var result = await _callRecordService.UpdateCallRecordAsync(callRecordId, request);

       if (result.IsError)
       {
           if (result.Error.Code == "call_record_not_found")
           {
               return NotFound(result.Error);
           }
           return BadRequest(result.Error);
       }

       return Ok(result.Value.ToCallRecordDto());
   }

   /// <summary>
   /// Updates an existing call record by its reference string.
   /// </summary>
   /// <param name="reference">The unique reference string of the call record to update.</param>
   /// <param name="request">The updated call record details.</param>
   /// <returns>The updated call record details.</returns>
   /// <response code="200">Call record updated successfully.</response>
   /// <response code="400">If the request parameters are invalid.</response>
   /// <response code="404">If the call record with the specified reference is not found.</response>
   /// <response code="500">If an internal server error occurs.</response>
   [HttpPut("reference/{reference}")]
   [ProducesResponseType(typeof(CallRecordDto), 200)]
   [ProducesResponseType(400)]
   [ProducesResponseType(404)] 
   [ProducesResponseType(500)]
   public async Task<IActionResult> UpdateCallRecordByReferenceAsync([FromRoute] string reference, [FromBody] UpdateCallRecordRequest request)
   {
       var result = await _callRecordService.UpdateCallRecordAsync(reference, request);

       if (result.IsError)
       {
           if (result.Error.Code == "call_record_not_found")
           {
               return NotFound(result.Error);
           }
           return BadRequest(result.Error);
       }

       return Ok(result.Value.ToCallRecordDto());
   }

   #endregion

   #region DELETE

   /// <summary>
   /// Deletes a specific call record by its unique identifier.
   /// </summary>
   /// <param name="callRecordId">The unique identifier of the call record to delete.</param>
   /// <returns>The deleted call record details.</returns>
   /// <response code="200">Call record deleted successfully.</response>
   /// <response code="400">If the request parameters are invalid.</response>
   /// <response code="404">If the call record with the specified ID is not found.</response>
   /// <response code="500">If an internal server error occurs.</response>
   [HttpDelete("{callRecordId:guid}")]
   [ProducesResponseType(typeof(CallRecordDto), 200)]
   [ProducesResponseType(400)]
   [ProducesResponseType(404)] 
   [ProducesResponseType(500)]
   public async Task<IActionResult> RemoveCallRecordByIdAsync([FromRoute] Guid callRecordId)
   {
       var result = await _callRecordService.RemoveCallRecordAsync(callRecordId);

       if (result.IsError)
       {
           if (result.Error.Code == "call_record_not_found")
           {
               return NotFound(result.Error);
           }
           return BadRequest(result.Error);
       }

       return Ok(result.Value.ToCallRecordDto());
   }

   /// <summary>
   /// Deletes a specific call record by its reference string.
   /// </summary>
   /// <param name="reference">The unique reference string of the call record to delete.</param>
   /// <returns>The deleted call record details.</returns>
   /// <response code="200">Call record deleted successfully.</response>
   /// <response code="400">If the request parameters are invalid.</response>
   /// <response code="404">If the call record with the specified reference is not found.</response>
   /// <response code="500">If an internal server error occurs.</response>
   [HttpDelete("reference/{reference}")]
   [ProducesResponseType(typeof(CallRecordDto), 200)]
   [ProducesResponseType(400)]
   [ProducesResponseType(404)] 
   [ProducesResponseType(500)]
   public async Task<IActionResult> RemoveCallRecordByReferenceAsync([FromRoute] string reference)
   {
       var result = await _callRecordService.RemoveCallRecordAsync(reference);

       if (result.IsError)
       {
           if (result.Error.Code == "call_record_not_found")
           {
               return NotFound(result.Error);
           }
           return BadRequest(result.Error);
       }

       return Ok(result.Value.ToCallRecordDto());
   }

   #endregion
}
