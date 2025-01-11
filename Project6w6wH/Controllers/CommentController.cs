using Project6w6wH.Models;
using Project6w6wH.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Project6w6wH.Controllers
{
    public class CommentController : ApiController
    {
        private Model db = new Model();

        [HttpPost]
        [JwtAuthFilter]
        [Route("api/reviews")]
        public IHttpActionResult PostComments()
        {

            try
            {
                var userId = (int)HttpContext.Current.Items["memberid"];

                var httpRequest = HttpContext.Current.Request; //從HttpRequest.Files取得前端傳來圖片

                int storeid = int.Parse(httpRequest.Form["placeID"]);
                int memberid = userId;
                string comment = httpRequest.Form["comment"] != null ? httpRequest.Form["comment"] : null;
                int starcount = int.Parse(httpRequest.Form["starCount"]);
                string tags = httpRequest.Form["tags"];


                var storesID = db.Stores.Find(storeid);
                if (storesID == null)
                {
                    return Ok(new
                    {
                        statusCode = 404,
                        status = false,
                        message = "無此店家"
                    });
                }

                var duplicateComment = db.Stores.Where(m => m.Id == storeid).SelectMany(m => m.StoreComments.Select(x => x.MemberId));
                if (duplicateComment.Contains(memberid))
                {
                    int commentIdForCommentPictures = db.StoreComments.Where(m => m.StoreId == storeid && m.MemberId == memberid).Select(x => x.Id).FirstOrDefault();

                    var reSC = db.StoreComments.FirstOrDefault(s => s.Id == commentIdForCommentPictures);
                    if (reSC != null)
                    {
                        reSC.Comment = comment;
                        reSC.ModifyTime = DateTime.Now;
                        reSC.Stars = starcount;
                        reSC.Label = tags;
                        db.SaveChanges();
                    }

                    //var picturesToDelete = db.CommentPictures.Where(c => c.CommentId == commentIdForCommentPictures).ToList();
                    //db.CommentPictures.RemoveRange(picturesToDelete);
                    //db.SaveChanges();
                    var response = new
                    {
                        statusCode = 200,
                        status = true,
                        message = "評論修改成功"
                    };

                    return Ok(response);

                }
                else
                {
                    var sc = new StoreComments
                    {
                        StoreId = storeid,
                        MemberId = memberid,
                        Comment = comment,
                        CreateTime = DateTime.Now,
                        Stars = starcount,
                        Label = tags
                    };
                    db.StoreComments.Add(sc);
                    db.SaveChanges();

                    if (httpRequest.Files.Count > 0)
                    {
                        int commentIdForCommentPictures = db.StoreComments.Where(m => m.StoreId == storeid && m.MemberId == memberid).Select(x => x.Id).FirstOrDefault();
                        var uploadedFiles = httpRequest.Files;
                        string uploadPath = ConfigurationManager.AppSettings["UploadPath"];
                        string[] allowedExtensions = { ".jpg", ".png" };
                        foreach (string fileKey in uploadedFiles)
                        {
                            var file = uploadedFiles[fileKey];
                            if (file != null && file.ContentLength > 0)
                            {
                                string fileExtension = Path.GetExtension(file.FileName).ToLower();
                                if (!allowedExtensions.Contains(fileExtension)) continue; //檢查檔案類型

                                const int maxFileSizeInBytes = 20 * 1024 * 1024; // 1MB
                                if (file.ContentLength > maxFileSizeInBytes)
                                {
                                    continue; // 檔案太大，跳過
                                }

                                string fileName = Path.GetFileName(file.FileName);
                                string savePath = Path.Combine(uploadPath, fileName);

                                if (File.Exists(savePath))
                                {
                                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                                    fileName = $"{fileNameWithoutExtension}_{timestamp}{fileExtension}";
                                    savePath = Path.Combine(uploadPath, fileName);
                                }

                                file.SaveAs(savePath);

                                var cp = new CommentPictures
                                {
                                    CommentId = commentIdForCommentPictures,
                                    PictureUrl = fileName,
                                    CreateTime = DateTime.Now
                                };
                                db.CommentPictures.Add(cp);
                            }
                        }
                        db.SaveChanges();
                    }


                    var response = new
                    {
                        statusCode = 200,
                        status = true,
                        message = "評論成功"
                    };

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    statusCode = 500,
                    status = false,
                    message = "伺服器錯誤，請稍後再試",
                    error = ex.Message
                };
                return Content(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }


        [HttpPost]
        [JwtAuthFilter]
        [Route("api/comments/delete")]
        public IHttpActionResult CommentsDelete([FromBody] ForComment Commentvalue)
        {
            try
            {
                int userId = (int)HttpContext.Current.Items["memberid"];

                var Comment = db.StoreComments.Find(Commentvalue.CommentId);
                if (Comment == null)
                {
                    return Ok(new
                    {
                        statusCode = 404,
                        status = false,
                        message = "無此評論"
                    });
                }

                int mId = db.StoreComments.Where(m => m.Id == Commentvalue.CommentId).Select(x => x.MemberId).FirstOrDefault();
                if (userId != mId)
                {
                    return Ok(new
                    {
                        statusCode = 404,
                        status = false,
                        message = "用戶無此評論"
                    });
                }

                var PictureUrllist = db.CommentPictures.Where(m => m.CommentId == Commentvalue.CommentId).Select(x => x.PictureUrl).ToList();
                if (PictureUrllist.Any())
                {
                    string uploadPath = ConfigurationManager.AppSettings["UploadPath"];
                    foreach (string PictureName in PictureUrllist)
                    {
                        string savePath = Path.Combine(uploadPath, PictureName);
                        if (File.Exists(savePath))
                        {
                            File.Delete(savePath); // 刪除舊檔案
                        }
                    }
                }

                db.StoreComments.Remove(Comment);
                db.SaveChanges();
                return Ok(new
                {
                    statusCode = 200,
                    status = true,
                    message = "刪除成功！"
                });
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    statusCode = 500,
                    status = false,
                    message = "伺服器錯誤，請稍後再試",
                    error = ex.Message
                };
                return Content(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }

        [HttpPost]
        [JwtAuthFilter]
        [Route("api/comments/repeat")]
        public IHttpActionResult Commentrepeat([FromBody] ForComment Commentvalue)
        {
            try
            {
                int userId = (int)HttpContext.Current.Items["memberid"];

                var duplicateComment = db.Stores.Where(m => m.Id == Commentvalue.StoreId).SelectMany(m => m.StoreComments.Where(x => x.MemberId == userId)).ToList();
                //Select(x => new { x.Label, x.Comment })

                if (!duplicateComment.Any())
                {
                    return Ok(new
                    {
                        statusCode = 404,
                        status = false,
                        message = "用戶未評論"
                    });
                }

                int Commentid = duplicateComment.FirstOrDefault().Id;
                string tags = duplicateComment.FirstOrDefault().Label;

                List<int> tagslist = new List<int>();

                string[] tagsstrlist = tags.Split(',');
                foreach (var tag in tagsstrlist)
                {
                    tagslist.Add(int.Parse(tag));
                }

                var cps = db.CommentPictures.Where(c => c.CommentId == Commentid).Select(x => x.PictureUrl).ToList();
                string uploadPath = ConfigurationManager.AppSettings["UploadPath"];
                List<string> plist = new List<string>();
                if (cps.Any())
                {
                    foreach (string PictureName in cps)
                    {
                        string savePath = Path.Combine(uploadPath, PictureName);
                        plist.Add(savePath);
                    }
                }



                var fc = new
                {
                    storeId = Commentvalue.StoreId,
                    userId = userId,
                    comment = duplicateComment.FirstOrDefault()?.Comment,
                    starCount = duplicateComment.FirstOrDefault().Stars,
                    tags = tagslist,
                    commentPictures = cps.Any() ? plist : cps,
                };

                return Ok(new
                {
                    statusCode = 200,
                    status = true,
                    message = "已評論過",
                    data = fc
                });
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    statusCode = 500,
                    status = false,
                    message = "伺服器錯誤，請稍後再試",
                    error = ex.Message
                };
                return Content(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }

        [HttpPost]
        [JwtAuthFilter]
        [Route("api/comments/report")]
        public IHttpActionResult CommentsReport([FromBody] ReportComment Reportvalue)
        {
            if (Reportvalue.ReportReason == "")
            {
                var response = new
                {
                    statusCode = 200,
                    status = true,
                    message = "請填入檢舉原因!",
                };
                return Ok(response);
            }
            if (Reportvalue.Type == "")
            {
                var response = new
                {
                    statusCode = 200,
                    status = true,
                    message = "請填入檢舉類別!",
                };
                return Ok(response);
            }
            try
            {
                int userId = (int)HttpContext.Current.Items["memberid"];

                var addReport = new CommnetReports
                {
                    CommentId = Reportvalue.CommentId,
                    ReportUserId = userId,
                    Type = Reportvalue.Type,
                    Comment = Reportvalue.ReportReason,
                    CreateTime = DateTime.Now,
                };
                db.CommnetReport.Add(addReport);
                db.SaveChanges();  // 儲存變更
                var response = new
                {
                    statusCode = 200,
                    status = true,
                    message = "評論檢舉成功!",
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    statusCode = 500,
                    status = false,
                    message = "伺服器錯誤，請稍後再試",
                    error = ex.Message
                };
                return Content(System.Net.HttpStatusCode.InternalServerError, errorResponse);
            }
        }


        public class ForComment
        {
            public int CommentId { get; set; }
            public int StoreId { get; set; }
            public string Comment { get; set; }
            public int StarCount { get; set; }
        }
        public class ReportComment
        {
            public int CommentId { get; set; }
            public string Type { get; set; }
            public string ReportReason { get; set; }
        }
    }
}
