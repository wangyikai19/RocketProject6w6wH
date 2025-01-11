using Project6w6wH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Project6w6wH.Controllers
{
    public class SearchConditionsController : ApiController
    {
        private Model db = new Model();
        // GET: api/SearchConditions
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/SearchConditions/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/SearchConditions
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/SearchConditions/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/SearchConditions/5
        public void Delete(int id)
        {
        }
        [HttpGet]
        [Route("api/searchconditions")]
        public IHttpActionResult GetSearchConditions()
        {
            try
            {
                using (var context = new Model())
                {
                    var conditions = context.SearchCondition.ToList();

                    var data = conditions.Select(condition => new
                    {
                        id = condition.Id,
                        group = condition.Group,
                        name = condition.MVal
                    }).ToList();
                    var response = new
                    {
                        statusCode = 200,
                        status = true,
                        message = "資料取得成功",
                        data
                    };
                    if (conditions == null || conditions.Count == 0)
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Ok(response);
                    }

                }
            }
            catch (Exception ex)
            {
                // 捕獲異常並返回具體錯誤訊息
                return InternalServerError(new Exception("詳細錯誤訊息", ex));
            }

        }
        [HttpGet]
        [Route("api/homepage/marquee")]
        public IHttpActionResult GethomeLabel()
        {

            var homeLabel = db.SearchRecord
                .GroupBy(r => r.Label)
                .Select(g => new
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Labelid = db.SearchCondition
                    .Where(sc => sc.MVal == g.Key)
                    .Select(sc => sc.Id)
                    .FirstOrDefault()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();


            string numstr = "";

            foreach (var item in homeLabel)
            {
                numstr += item.Labelid + ",";
            }
            numstr = numstr.TrimEnd(',');
            return Ok(new
            {
                statusCode = 200,
                status = true,
                message = "成功取得標籤",
                data = numstr
            });
        }
    }
}
