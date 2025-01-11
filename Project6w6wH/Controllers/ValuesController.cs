using Microsoft.Ajax.Utilities;
using Project6w6wH.Models;
using Project6w6wH.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Project6w6wH.Controllers
{
    public class ValuesController : ApiController
    {
        private Model db = new Model();
        // GET api/values
        [JwtAuthFilter]
        [Route("api/values")]
        public IEnumerable<string> Get()
        {
            var userId = (int)HttpContext.Current.Items["memberid"];
            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// 驗證登入狀態
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //驗證登入
        // Post api/auth
        [HttpPost]
        [Route("api/v1/auth")]
        public IHttpActionResult Auth([FromBody] authRequest request)
        {
            // 模型驗證
            if (!ModelState.IsValid)
            {
                var errorStr = new
                {
                    statusCode = "400",
                    status = false,
                    message = "錯誤的請求"
                };

                return Ok(errorStr);
            }

            // 檢查是否帶入 Authorization 標頭
            var authHeader = Request.Headers.Authorization;
            if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter) || authHeader.Scheme != "Bearer")
            {
                var errorStr = new
                {
                    statusCode = "401",
                    status = false,
                    message = "未提供有效的 Token"
                };

                return Ok(errorStr);
            }

            try
            {
                // 取出並驗證 JwtToken
                var userToken = JwtAuthUtil.GetToken(authHeader.Parameter);

                // 假設 JwtAuthUtil 驗證成功並返回有效的用戶資料
                JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
                string jwtToken = jwtAuthUtil.ExpRefreshToken(userToken); // 生成刷新效期的 Token

                var responseStr = new
                {
                    statusCode = "200",
                    status = true,
                    message = "用戶已登入",
                    //jwtToken // 將新的 JwtToken 包含於回應中
                };

                return Ok(responseStr);
            }
            catch (Exception ex)
            {
                var errorStr = new
                {
                    statusCode = "401",
                    status = false,
                    message = "無效的 Token",
                    error = ex.Message
                };

                return Ok(errorStr);
            }

        }
        public class authRequest
        {
            public string JWTtoken { get; set; }
        }
        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }
        [HttpGet]
        [Route("api/user/Email/{id}")]
        public IHttpActionResult MenberEmail(int id)
        {
            try
            {
                using (var context = new Model())
                {
                    var member = context.Member.FirstOrDefault(m => m.Id == id);
                    if (member == null)
                    {
                        return NotFound(); // 如果未找到會員，返回 404
                    }
                    return Ok(new { Message = member.Email });
                }
            }
            catch (Exception ex)
            {
                // 捕獲異常並返回具體錯誤訊息
                return InternalServerError(new Exception("詳細錯誤訊息", ex));
            }
        }
        // POST api/value
        public void Post([FromBody] string value)
        {
            using (var context = new Model())
            {
                var newMember = new Members
                {
                    Email = value,

                };

                context.Member.Add(newMember);
                context.SaveChanges();
            }
        }
    }
}
