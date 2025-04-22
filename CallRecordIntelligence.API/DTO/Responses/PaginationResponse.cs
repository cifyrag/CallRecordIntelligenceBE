namespace CallRecordIntelligence.API.DTO.Responses;

public record PaginationResponse<T>(
    List<T> Items,
    int? NextPage,
    int TotalPages,
    int Total
);

public static partial class PaginationExtensions
{
    public static PaginationResponse<T> ToPageResponse<T>(this List<T> items, int page, int pageSize, int total)
        => new
        (
            items,
            (pageSize * (page + 1)) >= total ? null : page + 1,
            (int)Math.Ceiling((double)total / pageSize),
            total
        );
}