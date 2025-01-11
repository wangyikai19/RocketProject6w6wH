using Project6w6wH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Project6w6wH.Controllers
{
    public class ConfigsController : ApiController
    {
        // GET: api/Configs
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Configs/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Configs
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Configs/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Configs/5
        public void Delete(int id)
        {
        }
        [HttpGet]
        [Route("api/communicatedownload")]
        public IHttpActionResult GetCommunicateDownload()
        {
            try
            {
                using (var context = new Model())
                {
                    // 查詢所有店家
                    var Communicate = context.Configs.Where(c => c.Group == "CommunicateDownload").ToList();
                    var result = Communicate.Select(store => new
                    {
                        store.PVal,
                        store.MVal
                    });
                    if (Communicate == null || Communicate.Count == 0)
                    {
                        return NotFound();
                    }
                    return Ok(new
                    {
                        result
                    });
                }
            }
            catch (Exception ex)
            {
                // 捕獲異常並返回具體錯誤訊息
                return InternalServerError(new Exception("詳細錯誤訊息", ex));
            }
        }
    }
}
