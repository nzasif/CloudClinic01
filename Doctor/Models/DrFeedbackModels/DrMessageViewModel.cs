using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.DrFeedbackModels
{
    public class DrMessagesModel
    {
        public int TotalCount { get; set; }
        public List<DrMessageViewModel> DrMessages { get; set; }
    }

    public class DrMessageViewModel
    {
        public Guid MessageId { get; set; }
        public string MessageText { get; set; }
        public string MessageDateTime { get; set; }
        public List<DrMessageReplyViewModel> DrMessageReplies { get; set; }
        public string PatName { get; set; }
        public string PatId { get; set; }
        public string PatAddress { get; set; }
        public string PatProfilePicUri { get; set; }
    }

    public class DrMessageReplyViewModel
    {
        public Guid ReplyId { get; set; }
        public string ReplyText { get; set; }
        public bool IsRead { get; set; }
        public string ReplyDate { get; set; }
    }
}
