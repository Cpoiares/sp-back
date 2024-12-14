namespace sp_back.models.Models.Error;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
}