using Project6w6wH.Models;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Web.Http;

namespace Project6w6wH.Controllers
{
    public class AuthController : Controller
    {
        private Model db = new Model();
        private const string RedirectUri = "http://localhost:44320/callback";
        // 產生 Google 登入連結
        public ActionResult LoginGoogle()
        {
            string Id = db.Configs.Where(c => c.Group == "ClientId").Select(c => c.MVal).FirstOrDefault();
            string Secret = db.Configs.Where(c => c.Group == "ClientSecret").Select(c => c.MVal).FirstOrDefault();
            var url = $"https://accounts.google.com/o/oauth2/v2/auth" +
                      $"?client_id={Id}" +
                      $"&redirect_uri={RedirectUri}" +
                      $"&response_type=code" +
                      $"&scope=openid%20email%20profile";

            return Redirect(url);
        }

        // 處理 Google 回呼
        public async Task<ActionResult> CallbackGoogle(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Content("Google authentication failed.");
            }

            // 交換 Token
            using (var client = new HttpClient())
            {
                string Id = db.Configs.Where(c => c.Group == "ClientId").Select(c => c.MVal).FirstOrDefault();
                string Secret = db.Configs.Where(c => c.Group == "ClientSecret").Select(c => c.MVal).FirstOrDefault();
                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
                {

                    Content = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", Id),
                    new KeyValuePair<string, string>("client_secret", Secret),
                    new KeyValuePair<string, string>("redirect_uri", RedirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                })
                };

                var tokenResponse = await client.SendAsync(tokenRequest);
                var tokenResult = await tokenResponse.Content.ReadAsAsync<dynamic>();

                // 解析 ID Token 並取得使用者資訊
                var idToken = tokenResult.id_token.ToString();
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

                // 返回使用者資訊
                return Json(new
                {
                    Name = payload.Name,
                    Email = payload.Email,
                    Picture = payload.Picture
                }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}
