﻿using Jose;
using Project6w6wH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Project6w6wH.Security
{
    public class JwtAuthUtil
    {
        private Model db = new Model();
        //private readonly ApplicationDbContext db = new ApplicationDbContext(); // DB 連線
        private static readonly string secretKey = "ILoveRocketCoding";
        /// <summary>
        /// 生成 JwtToken
        /// </summary>
        /// <param name="id">會員id</param>
        /// <returns>JwtToken</returns>
        public string GenerateToken(int id)
        {
            // 自訂字串，驗證用，用來加密送出的 key (放在 Web.config 的 appSettings)
            /*  string secretKey = WebConfigurationManager.AppSettings["TokenKey"]; */// 從 appSettings 取出
            var user = db.Member.FirstOrDefault(m => m.Id == id); // 進 DB 取出想要夾帶的基本資料
            //string secretKey = "ILoveRocketCoding";


            // payload 需透過 token 傳遞的資料 (可夾帶常用且不重要的資料)
            var payload = new Dictionary<string, object>
            {
                { "Id", id },
                { "Email", user.Email },
                { "Country", user.Country },
                { "Exp", DateTime.Now.AddHours(24).ToString() } // JwtToken 時效設定 30 分
            };

            // 產生 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }
        /// <summary>
        /// 將 Token 解密取得夾帶的資料
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetToken(string token)
        {
            return JWT.Decode<Dictionary<string, object>>(token, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
        }
        /// <summary>
        /// 生成只刷新效期的 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string ExpRefreshToken(Dictionary<string, object> tokenData)
        {
            //string secretKey = WebConfigurationManager.AppSettings["TokenKey"];
            // payload 從原本 token 傳遞的資料沿用，並刷新效期
            var payload = new Dictionary<string, object>
            {
                { "Id", (int)tokenData["Id"] },
                { "Exp", DateTime.Now.AddMinutes(30).ToString() } // JwtToken 時效刷新設定 30 分
            };

            //產生刷新時效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 生成無效 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string RevokeToken()
        {
            string secretKey = "RevokeToken"; // 故意用不同的 key 生成
            var payload = new Dictionary<string, object>
            {
                { "Id", 0 },
                { "Account", "None" },
                { "NickName", "None" },
                { "Image", "None" },
                { "Exp", DateTime.Now.AddDays(-15).ToString() } // 使 JwtToken 過期 失效
            };

            // 產生失效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }
    }
}