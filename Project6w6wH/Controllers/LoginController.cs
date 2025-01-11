using Project6w6wH.Models;
using Project6w6wH.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static Project6w6wH.Controllers.StoresController;
using System.IdentityModel.Tokens.Jwt;

namespace Project6w6wH.Controllers
{
    public class LoginController : ApiController
    {
        private Model db = new Model();

        [HttpPost]
        [Route("user/register")]
        public IHttpActionResult Register([FromBody] MemberInfo request)
        {
            if (request == null)
            {
                var response = new
                {
                    statusCode = 404,
                    status = false,
                    message = "送出失敗，請再試一次",
                };
                return Ok(response);
            }
            try
            {
                var member = new Members
                {
                    Name = request.UserName,
                    Email = request.Email,
                    Country = request.Country,
                    Photo = request.UserPhoto,
                    Gender = request.Gender,
                    Birthday = request.BirthDay,
                };
                db.Member.Add(member);
                db.SaveChanges();
                var response = new
                {
                    statusCode = 200,
                    status = true,
                    message = "註冊成功！",
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("詳細錯誤訊息", ex));
            }

        }

        [HttpGet]
        [Route("api/user/googlelogin")]
        public IHttpActionResult GoogleLogin(string id)
        {
            string jwt = id;

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;

                // 從 JWT 中提取一些信息
                if (jsonToken != null)
                {
                    var issuer = jsonToken.Issuer;
                    var audience = jsonToken.Audiences.FirstOrDefault();
                    var claims = jsonToken.Claims;
                    var emailClaim = claims.FirstOrDefault(c => c.Type == "email")?.Value;
                    var user = db.Member.FirstOrDefault(m => m.Email == emailClaim);
                    if (user != null)
                    {
                        JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
                        string jwtToken = jwtAuthUtil.GenerateToken(user.Id);
                        var data = new
                        {
                            userId = user.Id,
                            userName = user.Name,
                            userPhoto = user.Photo,
                            token = jwtToken,
                        };
                        var response = new
                        {
                            statusCode = 200,
                            status = true,
                            message = "登入成功！",
                            data,
                        };
                        return Ok(response);
                    }
                    else
                    {
                        var data = new
                        {
                            userName = claims.FirstOrDefault(c => c.Type == "name")?.Value,
                            email = claims.FirstOrDefault(c => c.Type == "email")?.Value,
                            userPhoto = claims.FirstOrDefault(c => c.Type == "picture")?.Value,
                        };
                        var response = new
                        {
                            statusCode = 200,
                            status = true,
                            message = "firstSignUp！",
                            data
                        };
                        return Ok(response);
                    }

                    //return Ok(new
                    //{
                    //    Status = true,
                    //    Issuer = issuer,
                    //    Audience = audience,
                    //    Claims = claims.Select(c => new { c.Type, c.Value }),
                    //    JwtToken = jwtToken
                    //});
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid token: " + ex.Message);
            }

            return Ok(new
            {
                statusCode = 404,
                status = false,
                message = "登入失敗,請再登入一次"
            });
        }

        public class MemberInfo
        {
            public string UserName { get; set; }
            public string Email { get; set; }
            public string Country { get; set; }
            public string UserPhoto { get; set; }
            public int Gender { get; set; }
            public DateTime BirthDay { get; set; }
        }
        public enum GenderType
        {
            男 = 0,
            女 = 1,
            其他 = 2
        }
    }
}
