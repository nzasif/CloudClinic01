using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class MessageReply
    {
        [Key]
        public Guid ReplyId { get; set; }
        public Guid MessageId { get; set; }
        public Message? Message { get; set; }
        public string? ReplyText { get; set; }
        public bool IsRead { get; set; }
        public DateTime ReplyDate { get; set; }
    }
}
