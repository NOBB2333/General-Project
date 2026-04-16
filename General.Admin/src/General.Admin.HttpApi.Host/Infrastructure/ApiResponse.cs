namespace General.Admin.Infrastructure;

public class ApiResponse<T>
{
    public int Code { get; set; }

    public T? Data { get; set; }

    public object? Error { get; set; }

    public string Message { get; set; } = "ok";

    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T> { Code = 0, Data = data, Error = null, Message = "ok" };
    }
}
