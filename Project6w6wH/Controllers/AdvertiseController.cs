using Project6w6wH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Project6w6wH.Controllers
{
    public class AdvertiseController : ApiController
    {
        private Model db = new Model();

        [HttpGet]
        [Route("home/advertise/get")]
        public IHttpActionResult GetHomeAd()
        {
            try
            {

                // 查詢所有店家
                var ad = db.Advertise.ToList();
                var data = ad.Where(a => a.Class == "Home").Select(a => new
                {
                    adID = a.Id,
                    url = a.Url,
                    photo = a.PictureUrl,
                }).ToList();
                if (data == null || data.Count == 0)
                {
                    var response = new
                    {
                        statusCode = 404,
                        status = false,
                        message = "找不到首頁廣告",
                        data
                    };
                    return Ok(response);
                }
                var result = new
                {
                    statusCode = 200,
                    status = true,
                    message = "成功回傳首頁廣告",
                    data
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("詳細錯誤訊息", ex));
            }
        }
    }
}
