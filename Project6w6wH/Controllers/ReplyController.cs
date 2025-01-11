using Project6w6wH.Models;
using Project6w6wH.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Project6w6wH.Controllers
{
    public class ReplyController : ApiController
    {
        private Model db = new Model();
        [HttpGet]
        [Route("api/getreply/{id}")]
        public IHttpActionResult GetReply(int Id)
        {
            try
            {
                using (var context = new Model())
                {
                    var request = Request;
                    int? memberId = null;
                    if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Bearer")
                    {
                        memberId = 0;
                    }
                    else
                    {
                        try
                        {
                            var jwtObject = JwtAuthUtil.GetToken(request.Headers.Authorization.Parameter);
                            memberId = int.Parse(jwtObject["Id"].ToString());
                        }
                        catch
                        {
                            var result = new
                            {
                                statusCode = 401,
                                status = false,
                                message = "Token 無效或已過期"
                            };
                            return Content(System.Net.HttpStatusCode.Unauthorized, result);
                        }
                    }
                    var replies = context.Reply.Where(c => c.CommentId == Id).Include(m=>m.Members).Include(r=>r.StoreComments).ToList();
                    var comment = context.StoreComments.FirstOrDefault(sc => sc.Id == Id);
                    var repliID= replies.Select(r => r.Id).ToList();
                    var replyLike = context.ReplyLike.ToList();
                    var like = replyLike.Where(l =>  repliID.Contains(l.ReplyId)).Count();
                    var searchCondition = context.SearchCondition.ToList();
                    var reply = replies.Select(r => new
                    {
                        replyId = r.Id,
                        userId = r.ReplyUserId,
                        userName = r.Members?.Name ?? null, // 空值處理
                        userPhoto = r.Members?.Photo ?? null, // 空值處理
                        comment = r.ReplyContent,
                        createTime = r.CreateTime,
                        badge = r.Members?.Badge ?? null, // 空值處理
                        country = r.Members?.Country ?? null, // 空值處理
                        likeCount = like,
                        isLike = replyLike.Any(rl => rl.LikeUserId == memberId) ? true : false,
                    }).ToList();
                    var data = new
                    {
                        commentId = comment.Id,
                        userId = comment.MemberId,
                        userName = comment.Members.Name,
                        userPhoto = comment.Members.Photo,
                        photos = comment.CommentPictures,
                        starCount = comment.Stars,
                        comment = comment.Comment,
                        createTime = comment.CreateTime.ToString(),
                        likeCount = like,
                        isLike = comment.CommentLikes.Any(cl => cl.LikeUserId == memberId),
                        tags = searchCondition.Where(condition => comment.Label.Split(',')
                       .Select(tag => int.Parse(tag.Trim())).ToList()
                       .Contains(condition.Id)).Select(condition => condition.MVal)
                        .ToList(),
                        badge = comment.Members.Badge,
                        country = comment.Members.Country,
                        reply

                    };
                    var response = new
                    {
                        statusCode = 200,
                        status = true,
                        message = "資料取得成功",
                        data
                    };
                    if (data == null)
                    {
                        var result = new
                        {
                            statusCode = 200,
                            status = true,
                            message = "此評論沒有留言",
                            data
                        };
                        return Ok(result);
                    }
                    return Ok(response);

                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"伺服器處理請求時發生錯誤: {ex.Message}", ex));
            }
        }
        public class SearchReply
        {
            public int CommentId { get; set; }
            public int UserId { get; set; }
        }

        [HttpPost]
        [JwtAuthFilter]
        [Route("api/comments/reply")]
        public IHttpActionResult Postreply([FromBody] Forreply replyvalue)
        {
            try
            {
                int userId = (int)HttpContext.Current.Items["memberid"];
                var now = DateTime.Now;
                string withMilliseconds = now.ToString("yyyy-MM-dd!HH:mm:ss.fff");

                var Comment = db.StoreComments.Find(replyvalue.CommentId);
                if (Comment == null)
                {
                    return Ok(new
                    {
                        statusCode = 404,
                        status = false,
                        message = "無此評論"
                    });
                }

                var ry = new Replies
                {
                    CommentId = replyvalue.CommentId,
                    ReplyUserId = userId,
                    ReplyContent = replyvalue.Comment,
                    CreateTime = DateTime.Now,
                    ReplyOnlyCode = withMilliseconds
                };
                db.Reply.Add(ry);
                db.SaveChanges();

                var repliesWithMembers = db.Reply.Where(m => m.ReplyOnlyCode == withMilliseconds).Include(m => m.Members).ToList();

                var dataList = new List<object>();
                foreach (var detail in repliesWithMembers)
                {
                    string savePath = null;
                    if (detail.Members.Photo != null)
                    {
                        string userPath = ConfigurationManager.AppSettings["UserPhoto"];
                        savePath = Path.Combine(userPath, detail.Members.Photo);
                    }

                    var datadictionary = new
                    {
                        replyId = detail.Id,
                        userId = detail.ReplyUserId,
                        userName = detail.Members.Name,
                        userPhoto = savePath == null ? null : savePath,
                        comment = detail.ReplyContent,
                        postedAt = detail.CreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        badge = "level1",
                        likeCount = 0,
                        isLike = false,
                    };
                    dataList.Add(datadictionary);

                    var ny = new Notifies
                    {
                        ReplyId = detail.Id,
                        ReplyUserId = detail.ReplyUserId,
                        CommentId = replyvalue.CommentId,
                        CommentUserId = Comment.MemberId,
                        Check = 0,
                        CreateTime = DateTime.Now,
                    };
                    db.Notify.Add(ny);
                }

                db.SaveChanges();
                var response = new
                {
                    statusCode = 200,
                    status = true,
                    message = "留言成功",
                    data = dataList[0]
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


        [HttpPost]
        [JwtAuthFilter]
        [Route("api/messages/delete")]
        public IHttpActionResult Deletemessages([FromBody] Forreply replyvalue)
        {
            try
            {
                int userId = (int)HttpContext.Current.Items["memberid"];
                var messages = db.Reply.Find(replyvalue.ReplyId);

                if (messages == null)
                {
                    return Ok(new
                    {
                        statusCode = 404,
                        status = false,
                        message = "無此留言"
                    });
                }

                int rId = db.Reply.Where(m => m.Id == replyvalue.ReplyId).Select(x => x.ReplyUserId).FirstOrDefault();
                if (userId != rId)
                {
                    return Ok(new
                    {
                        statusCode = 404,
                        status = false,
                        message = "用戶無此留言"
                    });
                }

                db.Reply.Remove(messages);
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

        public class Forreply
        {
            public int CommentId { get; set; }
            public string Comment { get; set; }
            public int ReplyId { get; set; }
        }

        public class data
        {
            public int replyID { get; set; }
            public int userID { get; set; }
            public string userName { get; set; }
            public string userPhoto { get; set; }
            public string comment { get; set; }
            public string createTime { get; set; }
            public string badge { get; set; }
            public int likeCount { get; set; }
            public bool isLike { get; set; }
        }

    }
}
