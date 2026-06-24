using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.DrFeedbackModels
{
    public class DrMessageReplyCreateModel
    {
        [Required]
        public Guid MessageId { get; set; }
        public string ReplyText { get; set; }
    }
}
