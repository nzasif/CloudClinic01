using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared
{
    public class ResponseModel
    {
        public string Status { get; set; }
        public ErrorModel Error { get; set; }
        public object Result { get; set; }
    }

    public class ErrorModel
    {
        public string Type { get; set; }
        public string Message { get; set; }
    }
}
