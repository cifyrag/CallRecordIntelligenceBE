
namespace CallRecordIntelligence.TEST
{
    public class CallRecordServiceTests
    {
        private readonly ILogger<CallRecordService> _logger;
        private readonly IGenericRepository<CallRecord> _callRecordRepository;
        private readonly CallRecordService _service;

        public CallRecordServiceTests()
        {
            _logger = Substitute.For<ILogger<CallRecordService>>();
            _callRecordRepository = Substitute.For<IGenericRepository<CallRecord>>();

            _service = new CallRecordService(_logger, _callRecordRepository);
        }

        #region GET Tests

        [Fact]
        public async Task GetCallRecordAsync_GivenValidId_ReturnsCallRecord()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var expectedCallRecord = new CallRecord { Id = testId, Reference = "REF123" };
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(expectedCallRecord));

            // Act
            var result = await _service.GetCallRecordAsync(testId);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(testId, result.Value.Id);
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetCallRecordAsync_GivenIdNotFound_ReturnsNotFound()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>((CallRecord) null)); 

            // Act
            var result = await _service.GetCallRecordAsync(testId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("call_record_not_found", result.Error.Code);
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetCallRecordAsync_GivenIdRepositoryThrowsException_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var exception = new Exception("Database connection failed");
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.GetCallRecordAsync(testId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetCallRecordAsync_GivenValidReference_ReturnsCallRecord()
        {
            // Arrange
            var testReference = "REF456";
            var expectedCallRecord = new CallRecord { Id = Guid.NewGuid(), Reference = testReference };
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(expectedCallRecord));

            // Act
            var result = await _service.GetCallRecordAsync(testReference);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(testReference, result.Value.Reference);
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetCallRecordAsync_GivenNullOrWhitespaceReference_ReturnsValidationError(string invalidReference)
        {
            // Act
            var result = await _service.GetCallRecordAsync(invalidReference);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("reference_is_required", result.Error.Code);
            await _callRecordRepository.DidNotReceive().GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetCallRecordAsync_GivenReferenceNotFound_ReturnsNotFound()
        {
            // Arrange
            var testReference = "NONEXISTENT";
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                 .Returns(new Result<CallRecord?>((CallRecord)null));

            // Act
            var result = await _service.GetCallRecordAsync(testReference);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("call_record_not_found", result.Error.Code);
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }
        
        [Fact]
        public async Task GetCallRecordAsync_GivenReferenceRepositoryThrowsException_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var testReference = "REF456";
            var exception = new Exception("Database connection failed");
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.GetCallRecordAsync(testReference);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }
        
        [Fact]
        public async Task GetCallRecordsAsync_ReturnsPaginatedList()
        {
            // Arrange
            var totalCount = 100;
            var pageSize = 10;
            var page = 2;
            var expectedItems = Enumerable.Range(0, pageSize).Select(i => new CallRecord { Id = Guid.NewGuid(), Reference = $"REF{i}" }).ToList();
            
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCount));
            _callRecordRepository.GetListAsync<CallRecord>(
                    Arg.Any<Expression<Func<CallRecord, bool>>>(),
                    Arg.Any<Func<IQueryable<CallRecord>, IOrderedQueryable<CallRecord>>>(),
                    Arg.Any<List<Func<IQueryable<CallRecord>, IIncludableQueryable<CallRecord, object>>>>(),
                    Arg.Any<Expression<Func<CallRecord, CallRecord>>>(),
                    page * pageSize,
                    pageSize)
                .Returns(new Result<IEnumerable<CallRecord>>(expectedItems));

            // Act
            var result = await _service.GetCallRecordsAsync(page, pageSize);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(pageSize, result.Value.Items.Count);
            Assert.Equal(totalCount, result.Value.Total);

            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).GetListAsync<CallRecord>(
                Arg.Any<Expression<Func<CallRecord, bool>>>(),
                Arg.Any<Func<IQueryable<CallRecord>, IOrderedQueryable<CallRecord>>>(),
                Arg.Any<List<Func<IQueryable<CallRecord>, IIncludableQueryable<CallRecord, object>>>>(),
                Arg.Any<Expression<Func<CallRecord, CallRecord>>>(),
                 page * pageSize,
                 pageSize);
        }

        [Fact]
        public async Task GetCallRecordsAsync_WhenNoRecordsExist_ReturnsEmptyPaginatedList()
        {
            // Arrange
            var totalCount = 0;
            var pageSize = 10;
            var page = 0;
            
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCount));
            _callRecordRepository.GetListAsync<CallRecord>(
                    Arg.Any<Expression<Func<CallRecord, bool>>>(),
                    Arg.Any<Func<IQueryable<CallRecord>, IOrderedQueryable<CallRecord>>>(),
                    Arg.Any<List<Func<IQueryable<CallRecord>, IIncludableQueryable<CallRecord, object>>>>(),
                    Arg.Any<Expression<Func<CallRecord, CallRecord>>>())
                 .Returns(new Result<IEnumerable<CallRecord>>(new List<CallRecord>()));


            // Act
            var result = await _service.GetCallRecordsAsync(page, pageSize);

            // Assert
             Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.Items);
            Assert.Equal(totalCount, result.Value.Total);

            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
             await _callRecordRepository.DidNotReceive().GetListAsync<CallRecord>(
                 Arg.Any<Expression<Func<CallRecord, bool>>>(),
                 Arg.Any<Func<IQueryable<CallRecord>, IOrderedQueryable<CallRecord>>>(),
                 Arg.Any<List<Func<IQueryable<CallRecord>, IIncludableQueryable<CallRecord, object>>>>(),
                 Arg.Any<Expression<Func<CallRecord, CallRecord>>>());
        }

        [Fact]
        public async Task GetCallRecordsAsync_WithFilters_CallsRepositoryWithCorrectFilter()
        {
            // Arrange
            var totalCount = 5;
            var pageSize = 10;
            var page = 0;
            var phoneNumber = "123";
            var start = DateTimeOffset.UtcNow.AddDays(-7);
            var end = DateTimeOffset.UtcNow;

            var expectedItems = Enumerable.Range(0, totalCount).Select(i => new CallRecord { Id = Guid.NewGuid(), CallerId = "12345", StartTime = start.AddDays(i) }).ToList();
            
             _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCount));
            _callRecordRepository.GetListAsync<CallRecord>(
                    Arg.Any<Expression<Func<CallRecord, bool>>>(),
                    Arg.Any<Func<IQueryable<CallRecord>, IOrderedQueryable<CallRecord>>>(),
                    Arg.Any<List<Func<IQueryable<CallRecord>, IIncludableQueryable<CallRecord, object>>>>(),
                    Arg.Any<Expression<Func<CallRecord, CallRecord>>>(),
                    page * pageSize,
                    pageSize)
                .Returns(new Result<IEnumerable<CallRecord>>(expectedItems));

            // Act
            var result = await _service.GetCallRecordsAsync(page, pageSize, phoneNumber, start, end);

            // Assert
             Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(totalCount, result.Value.Items.Count);
            
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).GetListAsync<CallRecord>(
                Arg.Any<Expression<Func<CallRecord, bool>>>(),
                Arg.Any<Func<IQueryable<CallRecord>, IOrderedQueryable<CallRecord>>>(),
                Arg.Any<List<Func<IQueryable<CallRecord>, IIncludableQueryable<CallRecord, object>>>>(),
                Arg.Any<Expression<Func<CallRecord, CallRecord>>>(),
                 page * pageSize,
                 pageSize);
        }

        [Fact]
        public async Task GetCallRecordsAsync_RepositoryThrowsException_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var exception = new Exception("Database error during count");
             _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.GetCallRecordsAsync();

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).LogError(exception, "Exception caught while getting call records list");
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.DidNotReceive().GetListAsync<CallRecord>(
                Arg.Any<Expression<Func<CallRecord, bool>>>(),
                Arg.Any<Func<IQueryable<CallRecord>, IOrderedQueryable<CallRecord>>>(),
                Arg.Any<List<Func<IQueryable<CallRecord>, IIncludableQueryable<CallRecord, object>>>>(),
                Arg.Any<Expression<Func<CallRecord, CallRecord>>>());
        }

        #endregion

        #region POST Tests (CSV Parsing - Complex, focus on key scenarios)

        [Fact]
        public async Task AddCallRecordsFromCsvAsync_GivenValidCsv_ParsesAndAddsRecords()
        {
            // Arrange
            var csvContent = "CallerId,Recipient,CallDate,CallTime,Duration,Cost,Reference,Currency\n" +
                             "111,222,01/01/2023,10:00:00,60,1.50,REF001,USD\n" +
                             "333,444,02/01/2023,11:30:00,120,3.00,REF002,EUR";
            var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var expectedRequests = new List<AddCallRecordRequest>
            {
                new AddCallRecordRequest
                {
                    CallerId = "111", Recipient = "222", StartTime = new DateTimeOffset(2023, 1, 1, 9, 59, 0, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2023, 1, 1, 10, 0, 0, TimeSpan.Zero), Cost = 1.50m, Reference = "REF001", Currency = "USD"
                },
                 new AddCallRecordRequest
                {
                    CallerId = "333", Recipient = "444", StartTime = new DateTimeOffset(2023, 1, 2, 11, 28, 0, TimeSpan.Zero),
                    EndTime = new DateTimeOffset(2023, 1, 2, 11, 30, 0, TimeSpan.Zero), Cost = 3.00m, Reference = "REF002", Currency = "EUR"
                }
            };
            
            _callRecordRepository.AddRangeAsync(Arg.Any<IEnumerable<CallRecord>>())
                .Returns(new Result<bool>(true));

            // Act
            var result = await _service.AddCallRecordsFromCsvAsync(csvStream);

            // Assert
             Assert.False(result.IsError);
            Assert.True(result.Value);
            
            await _callRecordRepository.Received(1).AddRangeAsync(Arg.Any<IEnumerable<CallRecord>>());
            
            await _callRecordRepository.Received(1).AddRangeAsync(Arg.Is<IEnumerable<CallRecord>>(
                records => records.Count() == 2 &&
                           records.Any(r => r.Reference == "REF001" && r.Cost == 1.50m) && 
                           records.Any(r => r.Reference == "REF002" && r.Cost == 3.00m) 
            ));

            _logger.DidNotReceiveWithAnyArgs().Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>()); 
             _logger.DidNotReceiveWithAnyArgs().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>()); 
        }
        
        [Fact]
        public async Task AddCallRecordsFromCsvAsync_GivenEmptyCsvStream_ReturnsValidationError()
        {
            // Arrange
            var csvContent = "";
            var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            // Act
            var result = await _service.AddCallRecordsFromCsvAsync(csvStream);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("VALIDATION_ERROR", result.Error.Code); 
            await _callRecordRepository.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<CallRecord>>());
            _logger.DidNotReceiveWithAnyArgs().Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            _logger.DidNotReceiveWithAnyArgs().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

         [Fact]
        public async Task AddCallRecordsFromCsvAsync_GivenCsvWithOnlyHeader_ReturnsValidationError()
        {
            // Arrange
            var csvContent = "CallerId,Recipient,CallDate,CallTime,Duration,Cost,Reference,Currency\n";
            var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            // Act
            var result = await _service.AddCallRecordsFromCsvAsync(csvStream);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("VALIDATION_ERROR", result.Error.Code); 
            await _callRecordRepository.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<CallRecord>>());
            _logger.DidNotReceiveWithAnyArgs().Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            
            _logger.DidNotReceiveWithAnyArgs().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }


        [Fact]
        public async Task AddCallRecordsFromCsvAsync_WhenRepositoryAddRangeFails_ReturnsErrorAndLogs()
        {
            // Arrange
            var csvContent = "CallerId,Recipient,CallDate,CallTime,Duration,Cost,Reference,Currency\n" +
                             "111,222,01/01/2023,10:00:00,60,1.50,REF001,USD";
            var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var repoError = Error.Unexpected("repository_add_range_failed");
            _callRecordRepository.AddRangeAsync(Arg.Any<IEnumerable<CallRecord>>())
                .Returns(new Result<bool>(repoError));

            // Act
            var result = await _service.AddCallRecordsFromCsvAsync(csvStream);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code); 
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).AddRangeAsync(Arg.Any<IEnumerable<CallRecord>>());
        }

        [Fact]
        public async Task AddCallRecordAsync_GivenValidRequest_AddsRecord()
        {
            // Arrange
            var request = new AddCallRecordRequest
            {
                CallerId = "caller", Recipient = "recipient", StartTime = DateTimeOffset.UtcNow,
                EndTime = DateTimeOffset.UtcNow.AddMinutes(10), Cost = 5.00m, Reference = "NEWREF", Currency = "GBP"
            };
            var expectedCallRecord = new CallRecord { Id = Guid.NewGuid(), Reference = request.Reference }; 
            
            _callRecordRepository.AddAsync(Arg.Any<CallRecord>())
                .Returns(new Result<CallRecord>(expectedCallRecord));

            // Act
            var result = await _service.AddCallRecordAsync(request);

            // Assert
             Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedCallRecord.Id, result.Value.Id);
            Assert.Equal(request.Reference, result.Value.Reference); 

            await _callRecordRepository.Received(1).AddAsync(Arg.Is<CallRecord>(cr =>
                cr.CallerId == request.CallerId &&
                cr.Recipient == request.Recipient &&
                cr.StartTime == request.StartTime &&
                cr.EndTime == request.EndTime &&
                cr.Cost == request.Cost &&
                cr.Reference == request.Reference &&
                cr.Currency == request.Currency
            ));
        }

        [Fact]
        public async Task AddCallRecordAsync_WhenRepositoryThrowsException_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var request = new AddCallRecordRequest { Reference = "TEST" };
            var exception = new Exception("Database error on add");
            _callRecordRepository.AddAsync(Arg.Any<CallRecord>())
                .Throws(exception);

            // Act
            var result = await _service.AddCallRecordAsync(request);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).LogError(exception, "Exception caught while adding call record");
             await _callRecordRepository.Received(1).AddAsync(Arg.Any<CallRecord>());
        }

        [Fact]
        public async Task AddCallRecordsRangeAsync_GivenValidRequests_AddsRecordsRange()
        {
            // Arrange
            var requests = new List<AddCallRecordRequest>
            {
                new AddCallRecordRequest { Reference = "REF_A" },
                new AddCallRecordRequest { Reference = "REF_B" },
            };
            
            _callRecordRepository.AddRangeAsync(Arg.Any<IEnumerable<CallRecord>>())
                .Returns(new Result<bool>(true));

            // Act
            var result = await _service.AddCallRecordsRangeAsync(requests);

            // Assert
             Assert.False(result.IsError);
            Assert.True(result.Value);

            await _callRecordRepository.Received(1).AddRangeAsync(Arg.Is<IEnumerable<CallRecord>>(
                records => records.Count() == 2 &&
                           records.Any(r => r.Reference == "REF_A") && 
                           records.Any(r => r.Reference == "REF_B") 
            ));
        }

        [Fact]
        public async Task AddCallRecordsRangeAsync_GivenEmptyRequestsList_CallsRepositoryAddRangeWithEmptyList()
        {
             // Arrange
            var requests = new List<AddCallRecordRequest>();
            
            _callRecordRepository.AddRangeAsync(Arg.Any<IEnumerable<CallRecord>>())
                .Returns(new Result<bool>(true));

            // Act
            var result = await _service.AddCallRecordsRangeAsync(requests);

            // Assert
             Assert.False(result.IsError);
            Assert.True(result.Value);

            await _callRecordRepository.Received(1).AddRangeAsync(Arg.Is<IEnumerable<CallRecord>>(records => !records.Any()));
        }


        [Fact]
        public async Task AddCallRecordsRangeAsync_WhenRepositoryThrowsException_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var requests = new List<AddCallRecordRequest> { new AddCallRecordRequest() };
            var exception = new Exception("Database error on add range");
            _callRecordRepository.AddRangeAsync(Arg.Any<IEnumerable<CallRecord>>())
                .Throws(exception);

            // Act
            var result = await _service.AddCallRecordsRangeAsync(requests);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).LogError(exception, "Exception caught while adding call records list");
             await _callRecordRepository.Received(1).AddRangeAsync(Arg.Any<IEnumerable<CallRecord>>());
        }

        #endregion

        #region PUT Tests

        [Fact]
        public async Task UpdateCallRecordAsync_GivenValidIdAndPartialRequest_UpdatesCorrectly()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingRecord = new CallRecord
            {
                Id = testId,
                CallerId = "OriginalCaller",
                Recipient = "OriginalRecipient",
                StartTime = DateTimeOffset.UtcNow.AddHours(-1),
                EndTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                Cost = 10.00m,
                Reference = "OriginalRef",
                Currency = "OLD"
            };
            var updateRequest = new UpdateCallRecordRequest
            {
                CallerId = "UpdatedCaller",
                Cost = 12.50m,
                EndTime = DateTimeOffset.UtcNow, 
                Reference = null 
            };
            var updatedRecord = new CallRecord { Id = testId, CallerId = "UpdatedCaller", Cost = 12.50m, EndTime = DateTimeOffset.UtcNow, Reference = existingRecord.Reference, Recipient = existingRecord.Recipient, StartTime = existingRecord.StartTime, Currency = existingRecord.Currency };
            
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(existingRecord)); 
            _callRecordRepository.UpdateAsync(Arg.Is<CallRecord>(cr => cr.Id == testId))
                 .Returns(new Result<CallRecord>(updatedRecord)); 


            // Act
            var result = await _service.UpdateCallRecordAsync(testId, updateRequest);

            // Assert
             Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(testId, result.Value.Id);
            Assert.Equal(updateRequest.CallerId, result.Value.CallerId); 
            Assert.Equal(existingRecord.Recipient, result.Value.Recipient); 
            Assert.Equal(updateRequest.Cost, result.Value.Cost); 
            Assert.Equal(existingRecord.Reference, result.Value.Reference);  
            Assert.Equal(existingRecord.Currency, result.Value.Currency); 

            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).UpdateAsync(Arg.Is<CallRecord>(cr =>
                 cr.Id == testId &&
                 cr.CallerId == updateRequest.CallerId &&
                 cr.Recipient == existingRecord.Recipient && 
                 cr.StartTime == existingRecord.StartTime &&
                 cr.EndTime == updateRequest.EndTime &&
                 cr.Cost == updateRequest.Cost &&
                 cr.Reference == existingRecord.Reference && 
                 cr.Currency == existingRecord.Currency
            ));
        }

         [Fact]
        public async Task UpdateCallRecordAsync_GivenValidIdAndFullRequest_UpdatesAllFields()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingRecord = new CallRecord
            {
                Id = testId,
                CallerId = "OriginalCaller",
                Recipient = "OriginalRecipient",
                StartTime = DateTimeOffset.UtcNow.AddHours(-1),
                EndTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                Cost = 10.00m,
                Reference = "OriginalRef",
                Currency = "OLD"
            };
            var updateRequest = new UpdateCallRecordRequest
            {
                CallerId = "NewCaller",
                Recipient = "NewRecipient",
                StartTime = DateTimeOffset.UtcNow.AddHours(-2),
                EndTime = DateTimeOffset.UtcNow.AddMinutes(-15),
                Cost = 20.00m,
                Reference = "NewRef",
                Currency = "NEW"
            };
             var updatedRecord = new CallRecord { Id = testId, CallerId = "NewCaller", Recipient = "NewRecipient", StartTime = updateRequest.StartTime!.Value, EndTime = updateRequest.EndTime!.Value, Cost = updateRequest.Cost!.Value, Reference = "NewRef", Currency = "NEW"};

            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(existingRecord)); 
            _callRecordRepository.UpdateAsync(Arg.Is<CallRecord>(cr => cr.Id == testId))
                 .Returns(new Result<CallRecord>(updatedRecord));


            // Act
            var result = await _service.UpdateCallRecordAsync(testId, updateRequest);

            // Assert
             Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(testId, result.Value.Id);
            Assert.Equal(updateRequest.CallerId, result.Value.CallerId);
            Assert.Equal(updateRequest.Recipient, result.Value.Recipient);
            Assert.Equal(updateRequest.StartTime, result.Value.StartTime);
            Assert.Equal(updateRequest.EndTime, result.Value.EndTime);
            Assert.Equal(updateRequest.Cost, result.Value.Cost);
            Assert.Equal(updateRequest.Reference, result.Value.Reference);
            Assert.Equal(updateRequest.Currency, result.Value.Currency);

             await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).UpdateAsync(Arg.Is<CallRecord>(cr =>
                 cr.Id == testId &&
                 cr.CallerId == updateRequest.CallerId &&
                 cr.Recipient == updateRequest.Recipient &&
                 cr.StartTime == updateRequest.StartTime &&
                 cr.EndTime == updateRequest.EndTime &&
                 cr.Cost == updateRequest.Cost &&
                 cr.Reference == updateRequest.Reference &&
                 cr.Currency == updateRequest.Currency
            ));
        }


        [Fact]
        public async Task UpdateCallRecordAsync_GivenIdNotFound_ReturnsNotFound()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var updateRequest = new UpdateCallRecordRequest { CallerId = "Updated" };
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>((CallRecord)null));

            // Act
            var result = await _service.UpdateCallRecordAsync(testId, updateRequest);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("call_record_not_found", result.Error.Code);
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.DidNotReceive().UpdateAsync(Arg.Any<CallRecord>());
        }

        [Fact]
        public async Task UpdateCallRecordAsync_GivenIdRepositoryThrowsExceptionOnGet_ReturnsUnexpectedErrorAndLogs()
        {
             // Arrange
            var testId = Guid.NewGuid();
            var updateRequest = new UpdateCallRecordRequest { CallerId = "Updated" };
            var exception = new Exception("Database error on get");
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.UpdateCallRecordAsync(testId, updateRequest);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.DidNotReceive().UpdateAsync(Arg.Any<CallRecord>());
        }

         [Fact]
        public async Task UpdateCallRecordAsync_GivenIdRepositoryThrowsExceptionOnUpdate_ReturnsUnexpectedErrorAndLogs()
        {
             // Arrange
            var testId = Guid.NewGuid();
            var existingRecord = new CallRecord { Id = testId };
            var updateRequest = new UpdateCallRecordRequest { CallerId = "Updated" };
            var exception = new Exception("Database error on update");

            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(existingRecord));
            _callRecordRepository.UpdateAsync(Arg.Any<CallRecord>())
                 .Throws(exception);

            // Act
            var result = await _service.UpdateCallRecordAsync(testId, updateRequest);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).UpdateAsync(Arg.Any<CallRecord>());
        }


        [Fact]
        public async Task UpdateCallRecordAsync_GivenValidReferenceAndPartialRequest_UpdatesCorrectly()
        {
            // Arrange
            var testReference = "REF_UPDATE";
            var existingRecord = new CallRecord
            {
                Id = Guid.NewGuid(),
                CallerId = "OriginalCaller",
                Recipient = "OriginalRecipient",
                StartTime = DateTimeOffset.UtcNow.AddHours(-1),
                EndTime = DateTimeOffset.UtcNow.AddMinutes(-30),
                Cost = 10.00m,
                Reference = testReference,
                Currency = "OLD"
            };
            var updateRequest = new UpdateCallRecordRequest
            {
                CallerId = "UpdatedCaller",
                Cost = 12.50m,
                EndTime = DateTimeOffset.UtcNow, 
                 Reference = null 
            };
            var updatedRecord = new CallRecord { Id = existingRecord.Id, CallerId = "UpdatedCaller", Cost = 12.50m, EndTime = DateTimeOffset.UtcNow, Reference = testReference, Recipient = existingRecord.Recipient, StartTime = existingRecord.StartTime, Currency = existingRecord.Currency };
            
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(existingRecord)); 
            _callRecordRepository.UpdateAsync(Arg.Is<CallRecord>(cr => cr.Reference == testReference))
                 .Returns(new Result<CallRecord>(updatedRecord));


            // Act
            var result = await _service.UpdateCallRecordAsync(testReference, updateRequest);

            // Assert
             Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(testReference, result.Value.Reference);
            Assert.Equal(updateRequest.CallerId, result.Value.CallerId); 
            Assert.Equal(existingRecord.Recipient, result.Value.Recipient); 
            Assert.Equal(updateRequest.Cost, result.Value.Cost); 
            Assert.Equal(existingRecord.Currency, result.Value.Currency); 

            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).UpdateAsync(Arg.Is<CallRecord>(cr =>
                 cr.Reference == testReference &&
                 cr.CallerId == updateRequest.CallerId &&
                 cr.Recipient == existingRecord.Recipient &&
                 cr.StartTime == existingRecord.StartTime &&
                 cr.EndTime == updateRequest.EndTime &&
                 cr.Cost == updateRequest.Cost &&
                 cr.Currency == existingRecord.Currency
            ));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateCallRecordAsync_GivenNullOrWhitespaceReference_ReturnsValidationError(string invalidReference)
        {
            // Arrange
            var updateRequest = new UpdateCallRecordRequest { CallerId = "Updated" };

            // Act
            var result = await _service.UpdateCallRecordAsync(invalidReference, updateRequest);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("reference_is_required", result.Error.Code);
            await _callRecordRepository.DidNotReceive().GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
             await _callRecordRepository.DidNotReceive().UpdateAsync(Arg.Any<CallRecord>());
        }

        [Fact]
        public async Task UpdateCallRecordAsync_GivenReferenceNotFound_ReturnsNotFound()
        {
            // Arrange
            var testReference = "NONEXISTENT";
            var updateRequest = new UpdateCallRecordRequest { CallerId = "Updated" };
             _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>((CallRecord)null));

            // Act
            var result = await _service.UpdateCallRecordAsync(testReference, updateRequest);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("call_record_not_found", result.Error.Code);
             await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.DidNotReceive().UpdateAsync(Arg.Any<CallRecord>());
        }

         [Fact]
        public async Task UpdateCallRecordAsync_GivenReferenceRepositoryThrowsExceptionOnGet_ReturnsUnexpectedErrorAndLogs()
        {
             // Arrange
            var testReference = "REF_UPDATE";
            var updateRequest = new UpdateCallRecordRequest { CallerId = "Updated" };
            var exception = new Exception("Database error on get");
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.UpdateCallRecordAsync(testReference, updateRequest);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.DidNotReceive().UpdateAsync(Arg.Any<CallRecord>());
        }

        [Fact]
        public async Task UpdateCallRecordAsync_GivenReferenceRepositoryThrowsExceptionOnUpdate_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var testReference = "REF_UPDATE";
            var existingRecord = new CallRecord { Reference = testReference };
            var updateRequest = new UpdateCallRecordRequest { CallerId = "Updated" };
            var exception = new Exception("Database error on update");

            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(existingRecord));

            _callRecordRepository.UpdateAsync(Arg.Any<CallRecord>())
                .Throws(exception);

            // Act
            var result = await _service.UpdateCallRecordAsync(testReference, updateRequest);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);

            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());

            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).UpdateAsync(Arg.Any<CallRecord>());
        }
        #endregion

        #region DELETE Tests

        [Fact]
        public async Task RemoveCallRecordAsync_GivenValidId_RemovesRecord()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var recordToRemove = new CallRecord { Id = testId, Reference = "REF_DELETE" };
            
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(recordToRemove));
             _callRecordRepository.RemoveAsync(Arg.Is<CallRecord>(cr => cr.Id == testId))
                .Returns(new Result<CallRecord>(recordToRemove)); 

            // Act
            var result = await _service.RemoveCallRecordAsync(testId);

            // Assert
             Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(testId, result.Value.Id);

            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).RemoveAsync(Arg.Is<CallRecord>(cr => cr.Id == testId));
        }

        [Fact]
        public async Task RemoveCallRecordAsync_GivenIdNotFound_ReturnsNotFound()
        {
             // Arrange
            var testId = Guid.NewGuid();
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>((CallRecord)null));

            // Act
            var result = await _service.RemoveCallRecordAsync(testId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("call_record_not_found", result.Error.Code);
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.DidNotReceive().RemoveAsync(Arg.Any<CallRecord>());
        }

        [Fact]
        public async Task RemoveCallRecordAsync_GivenIdRepositoryThrowsExceptionOnGet_ReturnsUnexpectedErrorAndLogs()
        {
             // Arrange
            var testId = Guid.NewGuid();
            var exception = new Exception("Database error on get for delete");
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.RemoveCallRecordAsync(testId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
             await _callRecordRepository.DidNotReceive().RemoveAsync(Arg.Any<CallRecord>());
        }

        [Fact]
        public async Task RemoveCallRecordAsync_GivenIdRepositoryThrowsExceptionOnRemove_ReturnsUnexpectedErrorAndLogs()
        {
             // Arrange
            var testId = Guid.NewGuid();
            var recordToRemove = new CallRecord { Id = testId };
            var exception = new Exception("Database error on remove");
            
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(recordToRemove));
             _callRecordRepository.RemoveAsync(Arg.Any<CallRecord>())
                .Throws(exception);

            // Act
            var result = await _service.RemoveCallRecordAsync(testId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).RemoveAsync(Arg.Is<CallRecord>(cr => cr.Id == testId));
        }
        
        [Fact]
        public async Task RemoveCallRecordAsync_GivenValidReference_RemovesRecord()
        {
            // Arrange
            var testReference = "REF_DELETE_BY_REF";
            var recordToRemove = new CallRecord { Id = Guid.NewGuid(), Reference = testReference };
            
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(recordToRemove));
             _callRecordRepository.RemoveAsync(Arg.Is<CallRecord>(cr => cr.Reference == testReference))
                .Returns(new Result<CallRecord>(recordToRemove));

            // Act
            var result = await _service.RemoveCallRecordAsync(testReference);

            // Assert
             Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Equal(testReference, result.Value.Reference);

            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).RemoveAsync(Arg.Is<CallRecord>(cr => cr.Reference == testReference));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task RemoveCallRecordAsync_GivenNullOrWhitespaceReference_ReturnsValidationError(string invalidReference)
        {
            // Act
            var result = await _service.RemoveCallRecordAsync(invalidReference);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("reference_is_required", result.Error.Code);
            await _callRecordRepository.DidNotReceive().GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
             await _callRecordRepository.DidNotReceive().RemoveAsync(Arg.Any<CallRecord>());
        }

        [Fact]
        public async Task RemoveCallRecordAsync_GivenReferenceNotFound_ReturnsNotFound()
        {
             // Arrange
            var testReference = "NONEXISTENT";
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>((CallRecord)null));

            // Act
            var result = await _service.RemoveCallRecordAsync(testReference);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("call_record_not_found", result.Error.Code);
             await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.DidNotReceive().RemoveAsync(Arg.Any<CallRecord>());
        }
         
         [Fact]
        public async Task RemoveCallRecordAsync_GivenReferenceRepositoryThrowsExceptionOnGet_ReturnsUnexpectedErrorAndLogs()
        {
             // Arrange
            var testReference = "REF_DELETE_BY_REF";
            var exception = new Exception("Database error on get for delete by ref");
             _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.RemoveCallRecordAsync(testReference);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.DidNotReceive().RemoveAsync(Arg.Any<CallRecord>());
        }

         [Fact]
        public async Task RemoveCallRecordAsync_GivenReferenceRepositoryThrowsExceptionOnRemove_ReturnsUnexpectedErrorAndLogs()
        {
             // Arrange
            var testReference = "REF_DELETE_BY_REF";
            var recordToRemove = new CallRecord { Reference = testReference };
            var exception = new Exception("Database error on remove by ref");
            
            _callRecordRepository.GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<CallRecord?>(recordToRemove));
             _callRecordRepository.RemoveAsync(Arg.Any<CallRecord>())
                .Throws(exception);

            // Act
            var result = await _service.RemoveCallRecordAsync(testReference);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).GetSingleAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).RemoveAsync(Arg.Is<CallRecord>(cr => cr.Reference == testReference));
        }

        #endregion
    }
}