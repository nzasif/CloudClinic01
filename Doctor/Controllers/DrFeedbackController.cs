using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models;
using CloudClinic.Doctor.Models.DrFeedbackModels;
using CloudClinic.Doctor.Services;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared.Services;
using CloudClinic.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CloudClinic.Doctor.Controllers
{
    [Authorize(Roles = Roles.DrRole + "," + Roles.DrAssistantRole)]
    [Route(Routes.DrFeedbackControllerRoute)]
    [ApiController]
    public class DrFeedbackController : ControllerBase
    {
        private object response;

        private CloudClinicDb db { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }
        private IHubContext<ClientsHub> hub { get; set; }

        public DrFeedbackController(
            CloudClinicDb Db,
            IDrDetailsProvider DrDetailsProvider,
            IHubContext<ClientsHub> hubContext)
        {
            db = Db;
            drDetailsProvider = DrDetailsProvider;
            hub = hubContext;
        }

        [HttpGet(Routes.GetMessagesRoute, Name = Routes.GetMessagesRoute)]
        public IActionResult GetMessages([FromQuery] int offset)
        {
            string drId = drDetailsProvider.GetDrId(User);
            var q = (from msg in db.Messages where msg.DrId == new Guid(drId) && msg.IsVisible == true
                     join patU in db.AppUsers on msg.PatId equals patU.Id
                     orderby msg.MessageDate descending
                     select new DrMessageViewModel
                     {
                         PatName = patU.FullName,
                         PatId = patU.Id,
                         PatAddress = patU.Street + ", " + patU.City + ", " + patU.Province,
                         PatProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, patU.ProfilePicName),
                         MessageId = msg.MessageId,
                         MessageText = msg.MessageText,
                         MessageDateTime = msg.MessageDate.ToShortDateString() + " " + msg.MessageDate.ToShortTimeString(),
                         DrMessageReplies = (from rep in db.MessageReplies
                                             where rep.MessageId == msg.MessageId
                                             select new DrMessageReplyViewModel
                                             {
                                                 ReplyId = rep.ReplyId,
                                                 ReplyText = rep.ReplyText,
                                                 ReplyDate = rep.ReplyDate.ToString(),
                                                 IsRead = rep.IsRead
                                             }).ToList()
                     });

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse(
                  "404",
                  new ErrorModel { Type = ErrorTypes.NotFound, Message = "no messages found" },
                  null);

                return Ok(response);
            }

            var list = (q.Count() > offset + 30) ? q.Skip(offset).Take(30).ToList() : q.ToList();
            var messages = new DrMessagesModel
            {
                TotalCount = q.Count(),
                DrMessages = list
            };

            response = HelperMethods.CreateResponse("200", null, new { messages });
            return Ok(response);
        }

        [HttpGet(Routes.GetMessageRoute, Name = Routes.GetMessageRoute)]
        public IActionResult GetMessage([FromQuery] Guid messageId)
        {
            string drId = drDetailsProvider.GetDrId(User);
            var q = (from msg in db.Messages
                     where msg.MessageId == messageId
                     && msg.DrId == new Guid(drId)
                     && msg.IsVisible == true
                     join patU in db.AppUsers on msg.PatId equals patU.Id
                     select new DrMessageViewModel
                     {
                         PatName = patU.FullName,
                         PatId = patU.Id,
                         PatAddress = patU.Street + ", " + patU.City + ", " + patU.Province,
                         PatProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, patU.ProfilePicName),
                         MessageId = msg.MessageId,
                         MessageText = msg.MessageText,
                         MessageDateTime = msg.MessageDate.ToShortDateString() + " " + msg.MessageDate.ToShortTimeString(),
                         DrMessageReplies = (from rep in db.MessageReplies
                                             where rep.MessageId == msg.MessageId
                                             select new DrMessageReplyViewModel
                                             {
                                                 ReplyId = rep.ReplyId,
                                                 ReplyText = rep.ReplyText,
                                                 ReplyDate = rep.ReplyDate.ToString(),
                                                 IsRead = rep.IsRead
                                             }).ToList()
                     });

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse(
                  "404",
                  new ErrorModel { Type = ErrorTypes.NotFound, Message = "no conversation with this patient" },
                  null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { message = q.First() });
            return Ok(response);
        }

        [HttpGet(Routes.ToggleReadMessageRoute, Name = Routes.ToggleReadMessageRoute)]
        public async Task<IActionResult> ToggleReadMessage([FromQuery] Guid messageId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            Message message = await db.Messages.FirstOrDefaultAsync(m => m.MessageId == messageId && m.DrId == new Guid(drId));

            if (message is null) 
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "Not foudn" }, null);
                return Ok(response);
            }

            message.IsRead = !message.IsRead;
            db.SaveChanges();

            response = HelperMethods.CreateResponse("404", null, new { isRead = message.IsRead });
            return Ok(response);
        }

        [HttpGet(Routes.RemoveMsgFromYourChatRoute, Name = Routes.RemoveMsgFromYourChatRoute)]
        public async Task<IActionResult> RemoveMsgFromYourChat([FromQuery] Guid messageId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            Message message = await db.Messages.FirstOrDefaultAsync(m => m.MessageId == messageId && m.DrId == new Guid(drId));
            
            if(message is null)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "Not foudn" }, null);
                return Ok(response);
            }

            message.IsVisible = false;
            db.SaveChanges();

            response = HelperMethods.CreateResponse("404", null, new { msg = "successed" });
            return Ok(response);
        }

        // used for create and update
        [HttpPost(Routes.DoReplyRoute, Name = Routes.DoReplyRoute)]
        public async Task<IActionResult> DoReply([FromBody] DrMessageReplyCreateModel model)
        {
            string drId = drDetailsProvider.GetDrId(User);

            var msg = db.Messages.FirstOrDefault(m => m.MessageId == model.MessageId);
            if (msg is null)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "Not foudn" }, null);
                return Ok(response);
            }

            MessageReply reply = new MessageReply
            {
                ReplyText = model.ReplyText,
                MessageId = model.MessageId,
                IsRead = false,
                ReplyDate = DateTime.Now
            };

            db.MessageReplies.Add(reply);
            await db.SaveChangesAsync();

            object replyView = new
            {
                drId,
                messageId = reply.MessageId,

                reply = new
                {
                    replyId = reply.ReplyId,
                    replyText = reply.ReplyText,
                    replyDate = reply.ReplyDate,
                }
            };

            await hub.Clients.User(msg.PatId).SendAsync("receiveReply", new { replyView });

            response = HelperMethods.CreateResponse("200", null, new { replyId = reply.ReplyId, replyDate = reply.ReplyDate });
            return Ok(response);
        }

        [HttpGet(Routes.RemoveReplyRoute, Name = Routes.RemoveReplyRoute)]
        public async Task<IActionResult> RemoveReply([FromQuery] Guid replyId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            MessageReply reply = await db.MessageReplies.FirstOrDefaultAsync(r => r.ReplyId == replyId);

            if (reply is null)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "Not foudn" }, null);
                return Ok(response);
            }

            db.MessageReplies.Remove(reply);
            await db.SaveChangesAsync();

            response = HelperMethods.CreateResponse("200", null, new { msg = "successed" });
            return Ok(response); ;
        }
    }
}