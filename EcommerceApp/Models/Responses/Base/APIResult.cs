using EcommerceApp.Constants;

namespace EcommerceApp.Models.Responses.Base
{
    public class APIResult<TData>
    {
        public bool Status { get; set; }
        public string Description { get; set; } = null;
        public TData Data { get; set; }
        public ErrorApiResult Error { get; set; }

        public class ErrorApiResult
        {
            public ProjectErrorCodes ErrorCode { get; set; }
            public string Description { get; set; }
        }

        public APIResult()
        {
        }

        public APIResult(ProjectErrorCodes code, string description = null)
        {
            Status = false;
            Error = new ErrorApiResult
            {
                ErrorCode = code,
                Description = description
            };
        }

        public APIResult(Exception exception, bool fullInfo = true)
        {
            Status = false;
            Error = new ErrorApiResult
            {
                ErrorCode = ProjectErrorCodes.Exception,
                Description = $"{exception.Message} {(fullInfo ? exception.StackTrace : "")}"
            };
        }

        public APIResult(TData data, string descr = null)
        {
            Status = true;
            Data = data;
            Description = descr;
            Error = null;
        }
    }
}
