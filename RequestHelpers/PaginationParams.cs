namespace API.RequestHelpers; 

public class PaginationParams {
    private const int MaxPageSize = 36;
    public int Page { get; set; } = 1;
    private int _pageSize = 12;

    public int PageSize {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}