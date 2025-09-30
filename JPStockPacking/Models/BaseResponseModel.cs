namespace JPStockPacking.Models
{
    public class BaseResponseModel<T>
    {
        public int Code { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T? Content { get; set; }
    }

    public class BaseResponseModel
    {
        public int Code { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}
