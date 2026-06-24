using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared.Services
{
    public class HelperMethods
    {
        private static string clientBaseUrl = "http://localhost:4200/";
        public static string GenerateProfilePicUri(HttpContext httpContext, string profilePicName)
        {
            if (string.IsNullOrEmpty(profilePicName))
            {
                return null;
            }
            string serverUrl = httpContext.Request.Scheme + "://" + httpContext.Request.Host;

            return serverUrl + "/ProfilePics/" + profilePicName;
        }

        public static string GenerateAttachmentUri(HttpContext httpContext, string attachmentName)
        {
            if (string.IsNullOrEmpty(attachmentName))
            {
                return null;
            }

            string serverUrl = httpContext.Request.Scheme + "://" + httpContext.Request.Host;

            return serverUrl + "/Attachments/" + attachmentName;
        }

        public static string SaveFile(IFormFile file, string uploadPath)
        {
            var rnd = new Random();
            string fileName = rnd.Next(5000).ToString();

            fileName = fileName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(file.FileName);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            using (FileStream fileStream = System.IO.File.Create(Path.Combine(uploadPath, fileName)))
            {
                file.CopyTo(fileStream);
            }

            return fileName;
        }

        public static string AddAttachment(IFormFile attachmentFile, IWebHostEnvironment environment)
        {
            switch (attachmentFile.ContentType)
            {
                case "image/jpeg":
                    break;
                case "image/png":
                    break;
                case "image/jpg":
                    break;
                case "doc/docx":
                    break;
                case "doc/pdf":
                    break;
                default:
                    return "error: Only png, jpeg, jpg image formates are allowed";
            }

            if (attachmentFile.Length > 7000000)
            {
                return "error: attachment file size should not exceeds 6mb";
            }

            string uploadPath = Path.Combine(environment.WebRootPath, "Attachments");

            return SaveFile(attachmentFile, uploadPath);
        }

        public static void DeleteAttachment(string attachmentName, IWebHostEnvironment environment)
        {
            string uploadPath = Path.Combine(environment.WebRootPath, "Attachments");
            string filePath = Path.Combine(uploadPath, attachmentName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static string AddProfilePic(IFormFile profilePic, IWebHostEnvironment environment)
        {
            switch (profilePic.ContentType)
            {
                case "image/jpeg":
                    break;
                case "image/png":
                    break;
                case "image/jpg":
                    break;
                default:
                    return "error: this type of file is not allowed";
            }

            // Todo: length limit will be reduced to 1mb
            if (profilePic.Length > 1000000 * 5)
            {
                return "error: max size limit exceeded.";
            }

            string uploadPath = Path.Combine(environment.WebRootPath, "ProfilePics");

            return SaveFile(profilePic, uploadPath);

        }
        public static void DeleteProfilePic(string PicName, IWebHostEnvironment environment)
        {
            string uploadPath = Path.Combine(environment.WebRootPath, "ProfilePics");
            string filePath = Path.Combine(uploadPath, PicName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static string CreateUrl(string relativeUrl)
        {
            return clientBaseUrl + relativeUrl;
        }

        public static object CreateResponse(string status, ErrorModel error, object result)
        {
            return new
            {
                response = new ResponseModel
                {
                    Status = status,
                    Error = error,
                    Result = result
                }
            };
        }
    }
}
