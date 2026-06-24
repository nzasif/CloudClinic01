using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared
{
    public static class ErrorTypes
    {
        public const string UnVerifiedEmail = "UnVerifiedEmail";
        public const string Duplicate = "Duplicate";
        public const string AppointmentNotAvailable = "AppointmentNotAvailable";
        public const string NotFound = "NotFound";
        public const string DrNotVerified = "DrNotVerified";
        public const string RemovedUser = "RemovedUser";
        public const string RemovedReport = "RemovedReport";
        public const string HiddenReport = "RemovedReport";
        public const string RoleAssigningFailed = "RoleAssigningFailed";
        public const string ServerError = "ServerError";
        public const string ValidationError = "ValidationError";
        public const string NotValidCredentials = "NotValidCredentials";
    }

    public static class ErrorMessages
    {
        public const string UserNotFound = "user not found";
    }
}
