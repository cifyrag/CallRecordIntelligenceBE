# CallRecordIntelligenceBE

- [Local environment](#local-environment)

- [Technology Choices](#technology-choices)

## Local environment
1. Use docker compose to setup a local database:
```
docker-compose -f dev/docker-compose.yaml up -d
```

2. Apply the latest database migrations:
```
dotnet ef database update --project CallRecordIntelligence.EF --startup-project CallRecordIntelligence.API
```
## 2. Technology Choices

The implementation utilizes the following key technologies and patterns:

* **.NET Core / ASP.NET Core:** Provides a modern, high-performance, and cross-platform framework for building backend services and APIs. Its built-in features like Dependency Injection and async/await support are fundamental to the design.
* **Dependency Injection (DI):** Used extensively to inject dependencies like the logger (`ILogger`) and the generic repository (`IGenericRepository`). This promotes loose coupling, making the code more testable and maintainable.
* **Generic Repository Pattern:** Abstracts the data access logic, allowing the service to interact with a data store through a clean interface (`IGenericRepository<CallRecord>`) without knowing the underlying database implementation details.
* **Asynchronous Programming (async/await):** All I/O bound operations (database calls, stream reading) are implemented using `async` and `await`. This ensures the application remains responsive and scalable by not blocking threads during waiting periods.
* **Result Pattern:** Instead of relying solely on exceptions for error handling, the service methods return a `Result<T>` type. This pattern makes success and failure outcomes explicit in the method signature, improves code clarity, and allows for detailed error information (`Error` type) to be passed back to the caller.
* **Structured Logging (`Microsoft.Extensions.Logging`):** Used for capturing operational information and errors. Structured logging makes it easier to analyze logs using modern logging aggregation tools.
* **DateTimeOffset:** Used for handling timestamps to ensure correct handling of time zones.
* **Manual CSV Parsing:** The CSV import functionality uses standard `.NET` `StreamReader` and string manipulation for parsing. This is a simple approach suitable for a known, simple CSV format.

## 3. Assumptions Made

Based on the provided code and the context of building an API service, the following assumptions have been made:

* **Underlying Data Store:** It is assumed that an underlying data store  is configured and accessible via the injected `IGenericRepository<CallRecord>`. The repository abstraction hides the specific data store technology.
* **`IGenericRepository` Implementation:** It is assumed that a concrete implementation of `IGenericRepository<CallRecord>` exists and provides the necessary asynchronous methods (`GetSingleAsync`, `CountAsync`, `GetListAsync`, `AddAsync`, `AddRangeAsync`, `RemoveAsync`) correctly interacting with the data store.
* **`CallRecord` Entity Structure:** It is assumed that a `CallRecord` class exists in the  namespace with properties like `Id` (Guid), `CallerId` (string), `Recipient` (string), `StartTime` (DateTimeOffset), `EndTime` (DateTimeOffset), `Cost` (decimal), `Reference` (string), and `Currency` (string), matching the data processed by the service.
* **Request/Response Models:** It is assumed that `AddCallRecordRequest`, `UpdateCallRecordRequest`, `Result`, `Error`, and `PaginationResponse` types exist and provide the expected structure and behavior for handling service input, output, and errors. The `.ToPageResponse` extension method is also assumed to exist.
* **CSV File Format:** A strict assumption is made about the CSV file format used in `AddCallRecordsFromCsvAsync`. It must have exactly 8 columns in the following order: CallerId, Recipient, Date (dd/MM/yyyy), Time (HH:mm:ss), Duration (seconds - integer), Cost (decimal), Reference, Currency. It is also assumed that the first row is a header that should be skipped and that fields do not contain commas or require complex quoting/escaping. Dates are parsed assuming `dd/MM/yyyy` format.
* **Error Code Definitions:** The specific error codes used (e.g., `"call_record_not_found"`, `"reference_is_required"`) are assumed to be defined and handled appropriately by the calling layers or error handling middleware.
* **Dependency Injection Setup:** It is assumed that the necessary services (`ILogger<CallRecordService>`, `IGenericRepository<CallRecord>`) are correctly registered in the application's Dependency Injection container at startup.

## 4. Future Considerations and Enhancements

Given more time, the following enhancements and considerations would improve the service's robustness, maintainability, and features:

* **Robust CSV Parsing:** Replace the manual CSV parsing logic with a dedicated, well-tested CSV parsing library (e.g., CsvHelper). This would handle more complex CSV structures (quoted fields, different delimiters, escaping) and provide more resilient parsing with better error reporting for malformed rows.
* **Detailed Error Handling:** While the `Result` pattern is used, many catch blocks return a generic `Error.Unexpected()`. More specific error types or error details should be captured and returned in the `Result.Error` property (e.g., `Error.Validation` with specific field errors during CSV parsing or input validation).
* **Input Validation:** Implement comprehensive validation for `AddCallRecordRequest` and `UpdateCallRecordRequest` models using data annotations or a validation library (e.g., FluentValidation). This would centralize and standardize input validation logic.
* **Business Logic Validation:** Add checks for business constraints (e.g., ensure `StartTime` is before `EndTime`, validate phone number formats if necessary, check for duplicate references before adding).
* **Unit and Integration Tests:** Write automated tests to verify the logic within the service methods, including success paths, error conditions (not found, validation), and integration with the repository layer (using mock repositories).
* **Mapping Library:** Introduce a mapping library (e.g., AutoMapper) to automate the mapping between request/response models and the `CallRecord` entity, reducing boilerplate code and potential manual mapping errors.
* **Idempotency:** Consider mechanisms to ensure idempotency for the CSV upload if necessary (e.g., detecting and skipping records based on reference or other unique identifiers that might already exist).
* **Concurrency Control:** If concurrent updates to the *same* call record entity are possible, implement optimistic or pessimistic concurrency control mechanisms in the repository layer.
* **Configuration:** Externalize configurable values (like default page size) into application configuration files (`appsettings.json`).
* **Stream Processing for Large CSVs:** For extremely large CSV files, consider processing the stream line by line without loading the entire file into memory first, potentially combined with batching inserts to the database.
