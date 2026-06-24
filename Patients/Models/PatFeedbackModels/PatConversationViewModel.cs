using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models.PatFeedbackModels
{
    public class PatConversationViewModel
    {
        public Guid DrId { get; set; }
        public string DrName { get; set; }
        public string DrProfilePicUri { get; set; }
        public List<PatMessageViewModel> PatMessages { get; set; }
    }

    public class PatMessageViewModel
    {
        public Guid MessageId { get; set; }
        public string MessageText { get; set; }
        public bool IsRead { get; set; }
        public string MessageDate { get; set; }

        public List<MessageReplyViewModel> MessageReplies { get; set; }
    }

    public class MessageReplyViewModel
    {
        public Guid ReplyId { get; set; }
        public string ReplyText { get; set; }
        public string ReplyDate { get; set; }
    }
}
