namespace CallRecordIntelligence.TEST
{
    public class StatisticServiceTests
    {
        private readonly ILogger<StatisticService> _logger;
        private readonly IGenericRepository<CallRecord> _callRecordRepository;
        private readonly StatisticService _service;

        public StatisticServiceTests()
        {
            _logger = Substitute.For<ILogger<StatisticService>>();
            _callRecordRepository = Substitute.For<IGenericRepository<CallRecord>>();

            _service = new StatisticService(_logger, _callRecordRepository);
        }

        #region GetAverageCallCostAsync Tests

        [Fact]
        public async Task GetAverageCallCostAsync_GivenFilter_ReturnsCalculatedAverageCost()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var totalCost = 150.00m;
            var totalCount = 10;
            _callRecordRepository.SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                .Returns(new Result<decimal>(totalCost));
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCount));

            // Act
            var result = await _service.GetAverageCallCostAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Math.Round(totalCost / totalCount, 3), result.Value);
            await _callRecordRepository.Received(1).SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetAverageCallCostAsync_GivenFilter_WithRounding()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var totalCost = 150.0015m;
            var totalCount = 10;
            _callRecordRepository.SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                .Returns(new Result<decimal>(totalCost));
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCount));

            // Act
            var result = await _service.GetAverageCallCostAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Math.Round(totalCost / totalCount, 3), result.Value);
            await _callRecordRepository.Received(1).SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }


        [Fact]
        public async Task GetAverageCallCostAsync_WhenNoCallsFound_ReturnsZero()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            _callRecordRepository.SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                 .Returns(new Result<decimal>(0m));
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(0));

            // Act
            var result = await _service.GetAverageCallCostAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(0m, result.Value);
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
            await _callRecordRepository.Received(1).SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
        }

        [Fact]
        public async Task GetAverageCallCostAsync_WhenRepositorySumAsyncReturnsError_ReturnsUnexpectedError()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var repoError = Error.Unexpected("repo_sum_failed");
            _callRecordRepository.SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                .Returns(new Result<decimal>(repoError));
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(10));

            // Act
            var result = await _service.GetAverageCallCostAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("error_summing_call_costs", result.Error.Code);
            await _callRecordRepository.Received(1).SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetAverageCallCostAsync_WhenRepositoryCountAsyncReturnsError_ReturnsUnexpectedError()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var repoError = Error.Unexpected("repo_count_failed");
             _callRecordRepository.SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                .Returns(new Result<decimal>(150m));
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(repoError));

            // Act
            var result = await _service.GetAverageCallCostAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("error_counting_calls_for_average_cost", result.Error.Code);
            await _callRecordRepository.Received(1).SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetAverageCallCostAsync_WhenExceptionOccurs_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var exception = new Exception("Database connection failed");
            _callRecordRepository.SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                .Throws(exception);

            // Act
            var result = await _service.GetAverageCallCostAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
            await _callRecordRepository.DidNotReceive().CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetAverageCallCostAsync_GivenFilterWithCriteria_CallsRepositoryWithPredicate()
        {
             // Arrange
            var filter = new StatisticsFilterDto
            {
                StartDate = DateTimeOffset.UtcNow.AddDays(-1),
                EndDate = DateTimeOffset.UtcNow,
                PhoneNumber = "123",
                Currency = "USD"
            };
            var totalCost = 150.00m;
            var totalCount = 10;
             _callRecordRepository.SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                .Returns(new Result<decimal>(totalCost));
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCount));

            // Act
            var result = await _service.GetAverageCallCostAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Math.Round(totalCost / totalCount, 3), result.Value);
            await _callRecordRepository.Received(1).SumAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }


        #endregion

        #region GetTotalCallCountAsync Tests

        [Fact]
        public async Task GetTotalCallCountAsync_GivenFilter_ReturnsTotalCount()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var totalCount = 42;
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCount));

            // Act
            var result = await _service.GetTotalCallCountAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(totalCount, result.Value);
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetTotalCallCountAsync_WhenRepositoryReturnsError_ReturnsRepositoryError()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var repoError = Error.Unexpected("repo_count_failed");
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(repoError));

            // Act
            var result = await _service.GetTotalCallCountAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(repoError, result.Error);
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetTotalCallCountAsync_WhenExceptionOccurs_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var exception = new Exception("Database timeout");
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.GetTotalCallCountAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

         [Fact]
        public async Task GetTotalCallCountAsync_GivenFilterWithCriteria_CallsRepositoryWithPredicate()
        {
             // Arrange
            var filter = new StatisticsFilterDto
            {
                PhoneNumber = "456"
            };
            var totalCount = 15;
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCount));

            // Act
            var result = await _service.GetTotalCallCountAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(totalCount, result.Value);
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        #endregion

        #region GetAverageCallDurationAsync Tests
        
        [Fact]
        public async Task GetAverageCallDurationAsync_WhenAverageDurationIsZeroAndCountIsZero_ReturnsTimeSpanZero()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            _callRecordRepository.AverageAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                .Returns(new Result<decimal>(0m));
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(0));

            // Act
            var result = await _service.GetAverageCallDurationAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(TimeSpan.Zero, result.Value);
             await _callRecordRepository.Received(1).AverageAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
             await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

         [Fact]
        public async Task GetAverageCallDurationAsync_WhenAverageDurationIsZeroAndCountIsNonZero_ReturnsTimeSpanZero()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            _callRecordRepository.AverageAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                .Returns(new Result<decimal>(0m));
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(5));

            // Act
            var result = await _service.GetAverageCallDurationAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(TimeSpan.Zero, result.Value);
             await _callRecordRepository.Received(1).AverageAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
             await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }


        [Fact]
        public async Task GetAverageCallDurationAsync_WhenRepositoryAverageAsyncReturnsError_ReturnsUnexpectedError()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var repoError = Error.Unexpected("repo_average_failed");
            _callRecordRepository.AverageAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                .Returns(new Result<decimal>(repoError));

            // Act
            var result = await _service.GetAverageCallDurationAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("error_calculating_average_duration_in_repository", result.Error.Code);
            await _callRecordRepository.Received(1).AverageAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
             await _callRecordRepository.DidNotReceive().CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetAverageCallDurationAsync_WhenExceptionOccurs_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var exception = new Exception("Database error during average");
             _callRecordRepository.AverageAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>())
                .Throws(exception);

            // Act
            var result = await _service.GetAverageCallDurationAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).AverageAsync(Arg.Any<Expression<Func<CallRecord, bool>>>(), Arg.Any<Expression<Func<CallRecord, decimal>>>());
             await _callRecordRepository.DidNotReceive().CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        #endregion

        #region GetLongestCallsAsync Tests
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetLongestCallsAsync_GivenNonPositiveCount_ReturnsEmptyListAndDoesNotCallRepository(int count)
        {
            // Arrange
            var filter = new StatisticsFilterDto();

            // Act
            var result = await _service.GetLongestCallsAsync(count, filter);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
            await _callRecordRepository.DidNotReceiveWithAnyArgs().GetListAsync<CallRecord>(
                Arg.Any<Expression<Func<CallRecord, bool>>>(),
                Arg.Any<Func<IQueryable<CallRecord>, IOrderedQueryable<CallRecord>>>(),
                Arg.Any<List<Func<IQueryable<CallRecord>, IIncludableQueryable<CallRecord, object>>>>(),
                Arg.Any<Expression<Func<CallRecord, CallRecord>>>(),
                Arg.Any<int>(),
                Arg.Any<int>());
        }
       
        #endregion

        #region GetCallsPerPeriodAsync Tests

        [Fact]
        public async Task GetCallsPerPeriodAsync_GivenValidFilter_ReturnsCalculatedAverage()
        {
            // Arrange
            var filter = new StatisticsPerPeriodFilterDto
            {
                StartDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2023, 1, 3, 0, 0, 0, TimeSpan.Zero),
                Granularity = StatisticsGranularity.Daily
            };
            var totalCalls = 10;
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCalls));

            // Act
            var result = await _service.GetCallsPerPeriodAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(totalCalls / 2.0, result.Value);
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetCallsPerPeriodAsync_GivenFilterWithMissingDates_ReturnsValidationError()
        {
            // Arrange
            var filter = new StatisticsPerPeriodFilterDto { Granularity = StatisticsGranularity.Daily };

            // Act
            var result = await _service.GetCallsPerPeriodAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("date_are_required_for_calls_per_period_calculation", result.Error.Code);
            await _callRecordRepository.DidNotReceive().CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetCallsPerPeriodAsync_WhenNoCallsFound_ReturnsZero()
        {
            // Arrange
            var filter = new StatisticsPerPeriodFilterDto
            {
                 StartDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                 EndDate = new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero),
                Granularity = StatisticsGranularity.Daily
            };
            var totalCalls = 0;
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCalls));

            // Act
            var result = await _service.GetCallsPerPeriodAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(0.0, result.Value);
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetCallsPerPeriodAsync_WhenRepositoryCountAsyncReturnsError_ReturnsUnexpectedError()
        {
            // Arrange
            var filter = new StatisticsPerPeriodFilterDto
            {
                 StartDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                 EndDate = new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero),
                Granularity = StatisticsGranularity.Daily
            };
            var repoError = Error.Unexpected("repo_count_failed");
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(repoError));

            // Act
            var result = await _service.GetCallsPerPeriodAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("error_getting_total_call_count_for_calls_per_period", result.Error.Code);
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetCallsPerPeriodAsync_WhenExceptionOccurs_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var filter = new StatisticsPerPeriodFilterDto
            {
                 StartDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                 EndDate = new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero),
                Granularity = StatisticsGranularity.Daily
            };
            var exception = new Exception("Database error during count");
            _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.GetCallsPerPeriodAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetCallsPerPeriodAsync_GivenSameStartDateAndEndDate_CalculatesTotalPeriodsAsOne()
        {
            // Arrange
             var startDate = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero);
            var filter = new StatisticsPerPeriodFilterDto
            {
                 StartDate = startDate,
                 EndDate = startDate,
                Granularity = StatisticsGranularity.Hourly
            };
            var totalCalls = 5;
             _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<int>(totalCalls));

            // Act
            var result = await _service.GetCallsPerPeriodAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(totalCalls / 1.0, result.Value);
            await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }


         [Fact]
         public async Task GetCallsPerPeriodAsync_GivenEndDateBeforeStartDate_CalculatesTotalPeriodsAsOne()
         {
             // Arrange
             var startDate = new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero);
             var endDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
             var filter = new StatisticsPerPeriodFilterDto
             {
                 StartDate = startDate,
                 EndDate = endDate,
                 Granularity = StatisticsGranularity.Daily
             };
             var totalCalls = 5;
             _callRecordRepository.CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>())
                 .Returns(new Result<int>(totalCalls));

             // Act
             var result = await _service.GetCallsPerPeriodAsync(filter);

             // Assert
             Assert.False(result.IsError);
             Assert.Equal(totalCalls / 1.0, result.Value);
             await _callRecordRepository.Received(1).CountAsync(Arg.Any<Expression<Func<CallRecord, bool>>>());
         }


        #endregion

        #region GetCallVolumeTrendAsync Tests

        [Fact]
        public async Task GetCallVolumeTrendAsync_GivenFilterWithMissingDates_ReturnsValidationError()
        {
            // Arrange
            var filter = new StatisticsVolumeTrendFilterDto { Granularity = StatisticsGranularity.Hourly };

            // Act
            var result = await _service.GetCallVolumeTrendAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("date_are_required_for_call_volume_trend", result.Error.Code);
            await _callRecordRepository.DidNotReceiveWithAnyArgs().GetListAsync<CallRecord>(
                Arg.Any<Expression<Func<CallRecord, bool>>>(),
                Arg.Any<Func<IQueryable<CallRecord>, IOrderedQueryable<CallRecord>>>(),
                Arg.Any<List<Func<IQueryable<CallRecord>, IIncludableQueryable<CallRecord, object>>>>(),
                Arg.Any<Expression<Func<CallRecord, CallRecord>>>(),
                Arg.Any<int>(),
                Arg.Any<int>());
        }

        [Fact]
        public async Task GetCallVolumeTrendAsync_WhenExceptionOccurs_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var filter = new StatisticsVolumeTrendFilterDto
            {
                 StartDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                 EndDate = new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero),
                Granularity = StatisticsGranularity.Daily
            };
            var exception = new Exception("Database error during list retrieval");
            _callRecordRepository.GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.GetCallVolumeTrendAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
             await _callRecordRepository.Received(1).GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

         [Theory]
         [InlineData(StatisticsGranularity.Hourly)]
         [InlineData(StatisticsGranularity.Daily)]
         [InlineData(StatisticsGranularity.Weekly)]
         [InlineData(StatisticsGranularity.Monthly)]
         [InlineData(StatisticsGranularity.Yearly)]
         public async Task GetCallVolumeTrendAsync_GivenVariousGranularities_CallsRepositoryWithPredicate(StatisticsGranularity granularity)
         {
              // Arrange
            var filter = new StatisticsVolumeTrendFilterDto
            {
                StartDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero),
                Granularity = granularity,
                PhoneNumber = "filtered",
                Currency = "GBP"
            };
            var calls = new List<CallRecord>
            {
                 new CallRecord { Id = Guid.NewGuid(), StartTime = new DateTimeOffset(2023, 6, 15, 12, 0, 0, TimeSpan.Zero), CallerId = "filtered", Currency = "GBP" },
            };
            _callRecordRepository.GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<IEnumerable<CallRecord>>(calls));

            // Act
            var result = await _service.GetCallVolumeTrendAsync(filter);

            // Assert
            Assert.False(result.IsError);
            await _callRecordRepository.Received(1).GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
         }


        #endregion

        #region GetTotalCostByCurrencyAsync Tests

        [Fact]
        public async Task GetTotalCostByCurrencyAsync_GivenFilter_ReturnsTotalCostGroupedByCurrency()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var calls = new List<CallRecord>
            {
                new CallRecord { Id = Guid.NewGuid(), Cost = 10.00m, Currency = "USD" },
                new CallRecord { Id = Guid.NewGuid(), Cost = 5.50m, Currency = "EUR" },
                new CallRecord { Id = Guid.NewGuid(), Cost = 12.00m, Currency = "USD" },
                new CallRecord { Id = Guid.NewGuid(), Cost = 3.00m, Currency = "EUR" },
                new CallRecord { Id = Guid.NewGuid(), Cost = 7.00m, Currency = "GBP" },
            };
            _callRecordRepository.GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<IEnumerable<CallRecord>>(calls));

            // Act
            var result = await _service.GetTotalCostByCurrencyAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            var totalCostByCurrency = result.Value;

            Assert.Equal(3, totalCostByCurrency.Count);
            Assert.Equal(22.00m, totalCostByCurrency["USD"]);
            Assert.Equal(8.50m, totalCostByCurrency["EUR"]);
            Assert.Equal(7.00m, totalCostByCurrency["GBP"]);

            await _callRecordRepository.Received(1).GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetTotalCostByCurrencyAsync_WhenNoCallsFound_ReturnsEmptyDictionary()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            _callRecordRepository.GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<IEnumerable<CallRecord>>(new List<CallRecord>()));

            // Act
            var result = await _service.GetTotalCostByCurrencyAsync(filter);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
             await _callRecordRepository.Received(1).GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetTotalCostByCurrencyAsync_WhenRepositoryGetListAsyncReturnsError_ReturnsUnexpectedError()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var repoError = Error.Unexpected("repo_getlist_failed");
            _callRecordRepository.GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<IEnumerable<CallRecord>>(repoError));

            // Act
            var result = await _service.GetTotalCostByCurrencyAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("error_fetching_calls_for_cost_by_currency", result.Error.Code);
             await _callRecordRepository.Received(1).GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        [Fact]
        public async Task GetTotalCostByCurrencyAsync_WhenExceptionOccurs_ReturnsUnexpectedErrorAndLogs()
        {
            // Arrange
            var filter = new StatisticsFilterDto();
            var exception = new Exception("Database error during list retrieval");
             _callRecordRepository.GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Throws(exception);

            // Act
            var result = await _service.GetTotalCostByCurrencyAsync(filter);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("UNEXPECTED_ERROR", result.Error.Code);
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
             await _callRecordRepository.Received(1).GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

         [Fact]
        public async Task GetTotalCostByCurrencyAsync_GivenFilterWithCriteria_CallsRepositoryWithPredicate()
        {
             // Arrange
            var filter = new StatisticsFilterDto
            {
                StartDate = DateTimeOffset.UtcNow.AddMonths(-1),
                PhoneNumber = "filtered phone"
            };
             var calls = new List<CallRecord>
            {
                 new CallRecord { Id = Guid.NewGuid(), Cost = 1.0m, Currency = "TEST" },
            };
            _callRecordRepository.GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>())
                .Returns(new Result<IEnumerable<CallRecord>>(calls));

            // Act
            var result = await _service.GetTotalCostByCurrencyAsync(filter);

            // Assert
            Assert.False(result.IsError);
             await _callRecordRepository.Received(1).GetListAsync<CallRecord>(Arg.Any<Expression<Func<CallRecord, bool>>>());
        }

        #endregion


    }
}