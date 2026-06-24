using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.SignalR
{
    public class HubNotificationModel
    {
        // this will used to find the receivers
        public List<string> ReceiversIds { get; set; }
        public string ClientMethodName { get; set; }
        // this data will be sent to the receviers
        // recever will extract their specific data like =>
        // data.labTestId, data.patName | data.reportId etc.
        public object Data { get; set; }
    }
}
