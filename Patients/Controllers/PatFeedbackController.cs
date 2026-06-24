using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Patients.Services;
using Microsoft.AspNetCore.Mvc;
using CloudClinic.Data.DbSets;
using CloudClinic.Patients.Models.PatFeedbackModels;
using CloudClinic.Shared.Services;
using Microsoft.EntityFrameworkCore;
using CloudClinic.Shared;
using CloudClinic.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using CloudClinic.Shared.AuthConstants;

namespace CloudClinic.Patients.Controllers
{
    [ApiController]
    [Authorize(Roles = Roles.NormalUserRole)]
    [Route(Routes.PatFeedbackControllerRoute)] 
    public class PatFeedbackController : ControllerBase
    {
        private object response;

        private CloudClinicDb db { get; set; }
        private IUserProfileProvider userInfoProvider { get; set; }
        private IHubContext<ClientsHub> hub { get; set; }

        public PatFeedbackController(
            CloudClinicDb Db,
            IHubContext<ClientsHub> hubContext,
            IUserProfileProvider UserInfoProvider)
        {
            db = Db;
            userInfoProvider = UserInfoProvider;
            hub = hubContext;
        }

        [HttpGet(Routes.PatGetConversationRoute, Name = Routes.PatGetConversationRoute)]
        public async Task<IActionResult> GetConversation([FromQuery] Guid drId)
        {
            string patId = userInfoProvider.GetUserId(User);
            var q = (from rpts in db.PatReports
                     where rpts.PatId == patId && rpts.DrId == drId
                     join dr in db.DrDetails on rpts.DrId equals dr.DrId
                     join drU in db.AppUsers on dr.UserId equals drU.Id
                     select new PatConversationViewModel
                     {
                         DrId = dr.DrId,
                         DrName = drU.FullName,
                         DrProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, drU.ProfilePicName),
                         PatMessages = (from msg in db.Messages where msg.DrId == drId && msg.PatId == patId
                                        ////orderby msg.MessageDate descending
                                        select new PatMessageViewModel 
                                        {
                                            MessageId = msg.MessageId,
                                            MessageText  = msg.MessageText,
                                            IsRead = msg.IsRead,
                                            MessageDate = msg.MessageDate.ToShortDateString() + " " + msg.MessageDate.ToShortTimeString(),
                                            MessageReplies = (from reply in db.MessageReplies where reply.MessageId == msg.MessageId
                                                            select new MessageReplyViewModel
                                                            {
                                                                ReplyId = reply.ReplyId,
                                                                ReplyText = reply.ReplyText,
                                                                ReplyDate = reply.ReplyDate.ToShortDateString() + " " +
                                                                reply.ReplyDate.ToShortTimeString()
                                                            }).ToList()
                                        }).ToList(),
                     });

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "not found" }, null);
                return Ok(response);
            }

            response = HelperMethods.CreateResponse("404", null, new { conversation = q.FirstOrDefault() });
            return Ok(response);
        }

        [HttpPost(Routes.SendMessageRoute, Name = Routes.SendMessageRoute)]
        public async Task<IActionResult> SendMessage([FromBody] PatMessageModel model)
        {
            string patId = userInfoProvider.GetUserId(User);

            // check if pat. has visited this dr.
            var r = db.Visits.Any(v => v.DrId == model.DrId && v.PatId == patId);
            if (!r)
            {
                response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "404", Message = "You have not visited this Dr." }, null);
                return Ok(response);
            }

            Message message = new Message
            {
                PatId = patId,
                DrId = model.DrId,
                MessageText = model.MessageText,
                IsRead = false,
                IsVisible = true,
                MessageDate = DateTime.Now
            };

            await db.Messages.AddAsync(message);
            await db.SaveChangesAsync();

            string drUserId = db.DrDetails.FirstOrDefault(dr => dr.DrId == model.DrId).UserId;
            await hub.Clients.User(drUserId).SendAsync("receiveMessage", new { messageId = message.MessageId });

            response = HelperMethods.CreateResponse("200", null, new { messageId = message.MessageId, messageDate = message.MessageDate });
            return Ok(response);
        }

        [HttpGet(Routes.RemoveMessageRoute, Name = Routes.RemoveMessageRoute)]
        public async Task<IActionResult> RemoveMessage([FromQuery] Guid messageId)
        {
            string patId = userInfoProvider.GetUserId(User);

            Message message = await db.Messages.FirstOrDefaultAsync(m => m.MessageId == messageId && m.PatId == patId);

            if (message is null)
            {
                response = HelperMethods.CreateResponse("200", new ErrorModel { Type = "404", Message = "not found" }, null);
                return Ok(response);
            }

            db.Messages.Remove(message);

            // now find all repliese to this
            var reps = db.MessageReplies.Where(r => r.MessageId == messageId);

            if(reps.Any())
            {
                db.MessageReplies.RemoveRange(reps);
            }

            await db.SaveChangesAsync();
            response = HelperMethods.CreateResponse("200", null, new { msg = "removed" });
            return Ok(response);
        }

        [HttpGet(Routes.ToggleReadReplyRoute, Name = Routes.ToggleReadReplyRoute)]
        public async Task<IActionResult> ToggleReadReply([FromQuery] Guid replyId)
        {
            //string userId = userInfoProvider.GetUserId(User);

            MessageReply reply = db.MessageReplies.FirstOrDefault(r => r.ReplyId == replyId);

            if (reply is null)
            {
                response = HelperMethods.CreateResponse("200", new ErrorModel { Type = "404", Message = "not found" }, null);
                return Ok(response);
            }

            reply.IsRead = !reply.IsRead;
            db.SaveChanges();

            response = HelperMethods.CreateResponse("200", null, new { msg = "toggled" });
            return Ok(response);
        }
    }
}