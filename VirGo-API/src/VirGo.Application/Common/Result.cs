namespace VirGo.Application.Common;

public sealed record Result<T>(bool IsSuccess, T? Value, string? Error);

public sealed record PageRequest(int Page = 1, int PageSize = 20)
{
    public int Skip => (Page - 1) * PageSize;
}

public sealed record Page<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, long Total);
