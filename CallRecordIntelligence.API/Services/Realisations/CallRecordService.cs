using System.Globalization;
namespace CallRecordIntelligence.API.Services.Realisations;

public class CallRecordService: ICallRecordService
{
    private readonly ILogger<CallRecordService> _logger;
    private readonly IGenericRepository<CallRecord> _callRecordRepository;

    public CallRecordService(
        ILogger<CallRecordService> logger, IGenericRepository<CallRecord> callRecordRepository)
    {
        _logger = logger;
        _callRecordRepository = callRecordRepository;
    }
    
    #region GET
    
    /// <summary>
    /// Retrieves a call record by its unique identifier.
    /// </summary>
    /// <param name="callRecordId">The unique identifier of the call record.</param>
    /// <returns>A containing the found call record or an error if not found or an unexpected error occurred.</returns>
    public async Task<Result<CallRecord>> GetCallRecordAsync(Guid callRecordId)
    {
        try
        {
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Id == callRecordId);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }

            return callRecord;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while getting call record by id: {callRecordId}",
                callRecordId);

            return Error.Unexpected();
        }
    }

    /// <summary>
    /// Retrieves a call record by its reference string.
    /// </summary>
    /// <param name="reference">The reference string of the call record.</param>
    /// <returns>A containing the found call record or an error if not found, the reference is missing, or an unexpected error occurred.</returns>
    public async Task<Result<CallRecord>> GetCallRecordAsync(string reference)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return Error.Validation(code: "reference_is_required");
            }
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Reference == reference);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }

            return callRecord;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while getting call record by reference: {reference}",
                reference);

            return Error.Unexpected();
        }
    }

    /// <summary>
    /// Retrieves a paginated list of call records with optional filtering.
    /// </summary>
    /// <param name="page">The page number (0-indexed).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="phoneNumber">Optional phone number filter (matches caller or recipient).</param>
    /// <param name="startTimestamp">Optional start timestamp filter.</param>
    /// <param name="endTimestamp">Optional end timestamp filter.</param>
    /// <returns>A containing the paginated list of call records or an unexpected error.</returns>
    public async Task<Result<PaginationResponse<CallRecord>>> GetCallRecordsAsync(
        int page = 0, 
        int pageSize = 50,
        string? phoneNumber = null, 
        DateTimeOffset? startTimestamp = null, 
        DateTimeOffset? endTimestamp = null)
    {
        try
        {
            startTimestamp = startTimestamp?.ToUniversalTime();
            endTimestamp = endTimestamp?.ToUniversalTime();
            
            var total = await _callRecordRepository.CountAsync(
                filter: c => 
                    (phoneNumber == null 
                     || (c.Recipient.Contains(phoneNumber) 
                         || c.CallerId.Contains(phoneNumber))) 
                    && (startTimestamp == null 
                        || c.StartTime >= startTimestamp) 
                    && (endTimestamp == null 
                        || c.EndTime <= endTimestamp));
            
            Result<IEnumerable<CallRecord>> callRecords = new List<CallRecord>();

            if (total.Value > 0)
            {
                callRecords = await _callRecordRepository.GetListAsync<CallRecord>(
                    filter: c =>
                        (phoneNumber == null
                         || (c.Recipient.Contains(phoneNumber)
                             || c.CallerId.Contains(phoneNumber)))
                        && (startTimestamp == null
                            || c.StartTime >= startTimestamp)
                        && (endTimestamp == null
                            || c.EndTime <= endTimestamp),
                    skip: page * pageSize,
                    take: pageSize);
            }
            
            if (total.Value == 0 || callRecords?.Value is null || !callRecords.Value.Any())
            {
                return new List<CallRecord>().ToPageResponse(page, pageSize, total.Value);
            }

            return callRecords.Value.ToList()
                .ToPageResponse(page, pageSize, total.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while getting call records list");

            return Error.Unexpected();
        }
    }

    #endregion
    
    #region POST

    /// <summary>
    /// Adds call records by parsing data from a CSV stream.
    /// </summary>
    /// <param name="csvStream">The stream containing the CSV data.</param>
    /// <returns>A indicating success or an error if parsing or adding failed.</returns>
    public async Task<Result<bool>> AddCallRecordsFromCsvAsync(Stream csvStream)
    {
        var callRecordRequests = new List<AddCallRecordRequest>();

        try
        {
            using (var reader = new StreamReader(csvStream))
            {
                if (!reader.EndOfStream)
                {
                    await reader.ReadLineAsync();
                }

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');

                    if (values.Length < 8)
                    {
                        _logger.LogWarning(
                            "Skipping CSV line with insufficient column count ({Count} instead of 8): {line}",
                            values.Length, line);
                        continue;
                    }

                    try
                    {
                        if (!DateTime.TryParseExact(values[2].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out DateTime callDate))
                        {
                            _logger.LogWarning("Skipping CSV line due to invalid date format: {line}", line);
                            continue;
                        }

                        if (!TimeSpan.TryParse(values[3].Trim(), CultureInfo.InvariantCulture, out TimeSpan callTime))
                        {
                            _logger.LogWarning("Skipping CSV line due to invalid time format: {line}", line);
                            continue;
                        }

                        DateTimeOffset endDateTime = new DateTimeOffset(callDate.Add(callTime), TimeSpan.Zero);

                        if (!int.TryParse(values[4].Trim(), out int durationSeconds))
                        {
                            _logger.LogWarning("Skipping CSV line due to invalid duration format: {line}", line);
                            continue;
                        }

                        TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);

                        DateTimeOffset startDateTime = endDateTime - duration;

                        if (!decimal.TryParse(values[5].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture,
                                out decimal cost))
                        {
                            _logger.LogWarning("Skipping CSV line due to invalid cost format: {line}", line);
                            continue;
                        }

                        var request = new AddCallRecordRequest
                        {
                            CallerId = values[0].Trim(),
                            Recipient = values[1].Trim(),
                            StartTime = startDateTime,
                            EndTime = endDateTime,
                            Cost = cost,
                            Reference = values[6].Trim(),
                            Currency = values[7].Trim()
                        };
                        callRecordRequests.Add(request);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error processing CSV line: {line}", line);
                        continue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading uploaded CSV stream.");
            return Error.Unexpected("An error occurred while processing the CSV file.");
        }

        if (!callRecordRequests.Any())
        {
            return Error.Validation("No valid call records found in the CSV file.");
        }

        var addRangeResult = await AddCallRecordsRangeAsync(callRecordRequests);

        if (addRangeResult.IsError)
        {
            _logger.LogError("Service error adding call records range: {error}", addRangeResult.Error);
            return Error.Unexpected("An error occurred while adding call records to the repository.");
        }

        return addRangeResult.Value;
    }

    /// <summary>
    /// Adds a single call record to the repository.
    /// </summary>
    /// <param name="request">The request object containing the call record details.</param>
    /// <returns>A containing the added call record or an unexpected error.</returns>
    public async Task<Result<CallRecord>> AddCallRecordAsync(AddCallRecordRequest request)
    {
        try
        {
            var newCallRecord = new CallRecord
            {
                CallerId = request.CallerId,
                Recipient = request.Recipient,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Cost = request.Cost,
                Reference = request.Reference,
                Currency = request.Currency
            };
            
            return await _callRecordRepository.AddAsync(newCallRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while adding call record");

            return Error.Unexpected();
        }
    }

    /// <summary>
    /// Adds a range of call records to the repository.
    /// </summary>
    /// <param name="requests">An enumerable of request objects containing the call record details.</param>
    /// <returns>A indicating success or an unexpected error.</returns>
    public async Task<Result<bool>> AddCallRecordsRangeAsync(IEnumerable<AddCallRecordRequest> requests)
    {
        try
        {
            var callrecords = requests.Select(request => new CallRecord
            {
                CallerId = request.CallerId,
                Recipient = request.Recipient,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Cost = request.Cost,
                Reference = request.Reference,
                Currency = request.Currency
            });
            
            return await _callRecordRepository.AddRangeAsync(callrecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while adding call records list");

            return Error.Unexpected();
        }
    }

    #endregion
    
    #region PUT
    
    /// <summary>
    /// Updates an existing call record by its unique identifier.
    /// </summary>
    /// <param name="callRecordId">The unique identifier of the call record to update.</param>
    /// <param name="request">The request object containing the updated call record details. Null properties will not update the corresponding field.</param>
    /// <returns>A containing the updated call record or an error if not found or an unexpected error occurred.</returns>
    public async Task<Result<CallRecord>> UpdateCallRecordAsync(Guid callRecordId, UpdateCallRecordRequest request)
    {
        try
        {
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Id == callRecordId);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }
            
            callRecord.Value.CallerId = request.CallerId ?? callRecord.Value.CallerId;
            callRecord.Value.Recipient = request.Recipient ?? callRecord.Value.Recipient;
            callRecord.Value.StartTime = request.StartTime ?? callRecord.Value.StartTime;
            callRecord.Value.EndTime = request.EndTime ?? callRecord.Value.EndTime;
            callRecord.Value.Cost = request.Cost ?? callRecord.Value.Cost;
            callRecord.Value.Reference = request.Reference ?? callRecord.Value.Reference;
            callRecord.Value.Currency = request.Currency ?? callRecord.Value.Currency;
            
            return await _callRecordRepository.UpdateAsync(callRecord.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while updating call record by id: {callRecordId}",
                callRecordId);

            return Error.Unexpected();
        }
    }

    /// <summary>
    /// Updates an existing call record by its reference string.
    /// </summary>
    /// <param name="reference">The reference string of the call record to update.</param>
    /// <param name="request">The request object containing the updated call record details. Null properties will not update the corresponding field.</param>
    /// <returns>A containing the updated call record or an error if not found, the reference is missing, or an unexpected error occurred.</returns>
    public async Task<Result<CallRecord>> UpdateCallRecordAsync(string reference, UpdateCallRecordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return Error.Validation(code: "reference_is_required");
            }
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Reference == reference);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }
            
            callRecord.Value.CallerId = request.CallerId ?? callRecord.Value.CallerId;
            callRecord.Value.Recipient = request.Recipient ?? callRecord.Value.Recipient;
            callRecord.Value.StartTime = request.StartTime ?? callRecord.Value.StartTime;
            callRecord.Value.EndTime = request.EndTime ?? callRecord.Value.EndTime;
            callRecord.Value.Cost = request.Cost ?? callRecord.Value.Cost;
            callRecord.Value.Currency = request.Currency ?? callRecord.Value.Currency;
            
            return await _callRecordRepository.UpdateAsync(callRecord.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while updating call record by reference: {reference}",
                reference);

            return Error.Unexpected();
        }
    }
    
    #endregion
    
    #region DELETE
    
    /// <summary>
    /// Removes a call record by its unique identifier.
    /// </summary>
    /// <param name="callRecordId">The unique identifier of the call record to remove.</param>
    /// <returns>A containing the removed call record or an error if not found or an unexpected error occurred.</returns>
    public async Task<Result<CallRecord>> RemoveCallRecordAsync(Guid callRecordId)
    {
        try
        {
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Id == callRecordId);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }
            
            return await _callRecordRepository.RemoveAsync(callRecord.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while deleting call record by id: {callRecordId}",
                callRecordId);

            return Error.Unexpected();
        }
    }

    /// <summary>
    /// Removes a call record by its reference string.
    /// </summary>
    /// <param name="reference">The reference string of the call record to remove.</param>
    /// <returns>A containing the removed call record or an error if not found, the reference is missing, or an unexpected error occurred.</returns>
    public async Task<Result<CallRecord>> RemoveCallRecordAsync(string reference)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return Error.Validation(code: "reference_is_required");
            }
            var callRecord = await _callRecordRepository.GetSingleAsync<CallRecord>(
                c => c.Reference == reference);

            if (callRecord?.Value is null)
            {
                return Error.NotFound(code: "call_record_not_found");
            }
            
            return await _callRecordRepository.RemoveAsync(callRecord.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught while deleting call record by reference: {reference}",
                reference);

            return Error.Unexpected();
        }
    }
    
    #endregion
    
}