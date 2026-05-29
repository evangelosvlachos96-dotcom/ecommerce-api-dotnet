using EcommerceApp.Utils;

namespace EcommerceApp.Models.DTO.Base
{
    public class InternalDataTransfer<T>
    {
        public bool Status { get; set; }
        public T Data { get; set; }
        public InternalErrorObject Error { get; set; }

        public InternalDataTransfer()
        {
        }

        public InternalDataTransfer(T data)
        {
            Status = true;
            Data = data;
        }

        public InternalDataTransfer(Exception e)
        {
            Status = false;
            Error = new InternalErrorObject(e);
        }

        public InternalDataTransfer(Exception e, string description)
        {
            Status = false;
            Error = new InternalErrorObject(e, description);
        }

        public InternalDataTransfer(bool status, string description, string details = null)
        {
            Status = status;
            if (!status)
            {
                Error = new InternalErrorObject(description, details);
            }
        }

        public InternalDataTransfer(InternalErrorObject error)
        {
            Status = false;
            Error = error;

        }
        public class InternalErrorObject
        {
            public string Error { get; set; }
            public string Description { get; set; }
            public string Details { get; set; }
            public bool IsExceptionTypeError { get; set; }

            public InternalErrorObject()
            {
            }

            public InternalErrorObject(Exception e)
            {
                Error = e.Message;
                Description = e.StackTrace;
                IsExceptionTypeError = true;
            }

            public InternalErrorObject(Exception e, string description)
            {
                if (description == null)
                {
                    Error = e.Message;
                    Description = e.StackTrace;
                    IsExceptionTypeError = true;
                }
                else
                {
                    Error = $"Message: { e.Message }, StackTrace: { e.StackTrace }";
                    Description = description;
                    IsExceptionTypeError = true;
                }

            }

            public InternalErrorObject(string description, string details = null)
            {
                Error = "Error";
                Description = description;
                Details = details;
                IsExceptionTypeError = false;
            }

            public TheTypeError<TypeError> GetErrorTypeByDescription<TypeError>() where TypeError : Enum
            {
                if (IsExceptionTypeError)
                {
                    throw new Exception($"{Error} - {Description}");
                }
                return new TheTypeError<TypeError>(Description.GetValueByName<TypeError>());
            }

            public TheTypeError<TypeError> GetErrorTypeByErrorStr<TypeError>() where TypeError : Enum
            {
                if (IsExceptionTypeError)
                {
                    throw new Exception($"{Error} - {Description}");
                }
                return new TheTypeError<TypeError>(Error.GetValueByName<TypeError>());
            }

            public class TheTypeError<TypeError> where TypeError : Enum
            {
                public TheTypeError()
                {
                }

                public TheTypeError(TypeError typeError)
                {
                    Value = typeError;
                }

                public TypeError Value { get; set; }
            }
        }
    }
}
