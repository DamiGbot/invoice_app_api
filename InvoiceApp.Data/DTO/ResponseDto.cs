using System.Collections;

namespace InvoiceApp.Data.DTO
{
    public class ResponseDto<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; } = string.Empty;
        public int? Count
        {
            get
            {
                if (Result is IEnumerable enumerable)
                {
                    return enumerable.Cast<object>().Count();
                }

                return null;
            }
        }
        public T? Result { get; set; }
    }
}
