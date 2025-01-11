using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project6w6wH.Models;
using Project6w6wH.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Optimization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static Google.Apis.Requests.BatchRequest;
using static Project6w6wH.Controllers.ReplyController;

namespace Project6w6wH.Controllers
{
    public class StoresController : ApiController
    {
        private Model db = new Model();
        // GET: api/Stores
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Stores/5
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        [Route("api/addstore")]
        public async Task<IHttpActionResult> AddStore([FromBody] AddRequest request)
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
            if (request.DisplayName == null)
            {
                var response = new
                {
                    statusCode = 404,
                    status = false,
                    message = "店家名稱為必填欄位!",
                };
                return Ok(response);
            }

            try
            {
                // Google Maps API 參數
                string googleApiKey = "AIzaSyDoKBrywIWBKHUyfZMGq_YX9kG_DEwDrWQ"; // 請替換為您的有效 Google API 密鑰
                string placeDetailsUrl = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={request.PlaceId}&key={googleApiKey}";
                var businessHours = "";
                string addressEn = string.Empty;
                string storeUrl = string.Empty;
                string phone = string.Empty;
                List<string> openingHours = null;
                using (var client = new HttpClient())
                {
                    HttpResponseMessage apiResponse = await client.GetAsync(placeDetailsUrl);

                    if (!apiResponse.IsSuccessStatusCode)
                    {
                        return Ok(new { statusCode = 400, status = false, message = "Google API 請求失敗" });
                    }

                    // 解析 Google API 回應數據
                    var responseContent = await apiResponse.Content.ReadAsStringAsync();
                    var json = JObject.Parse(responseContent);

                    // 確保地址存在
                    addressEn = json["result"]?["formatted_address"]?.ToString();
                    if (string.IsNullOrEmpty(addressEn))
                    {
                        addressEn = "";
                    }
                    openingHours = json["result"]?["opening_hours"]?["weekday_text"]?.ToObject<List<string>>();
                    if (openingHours == null || openingHours.Count == 0)
                    {
                        openingHours = null;
                    }
                    // 格式化並返回營業時間
                    if (openingHours != null)
                    {
                        var daysOfWeek = new Dictionary<string, string>
                        {
                            { "Sunday", "" },
                            { "Monday", "" },
                            { "Tuesday", "" },
                            { "Wednesday", "" },
                            { "Thursday", "" },
                            { "Friday", "" },
                            { "Saturday", "" }
                        };

                        foreach (var dayHours in openingHours)
                        {
                            var parts = dayHours.Split(':');
                            if (parts.Length == 2)
                            {
                                var day = parts[0].Trim();
                                var time = parts[1].Trim();

                                if (daysOfWeek.ContainsKey(day))
                                {
                                    daysOfWeek[day] = time;
                                }
                            }
                        }

                        // 序列化字典為 JSON 字符串
                        businessHours = JsonConvert.SerializeObject(daysOfWeek);
                    }
                    storeUrl = json["result"]?["website"]?.ToString();
                    if (string.IsNullOrEmpty(storeUrl))
                    {
                        storeUrl = "";  // 如果無預約網址，則設為空字串
                    }
                    phone = json["result"]?["formatted_phone_number"]?.ToString();
                    if (string.IsNullOrEmpty(phone))
                    {
                        phone = "";  // 如果無店家電話，則設為空字串
                    }
                }
                using (var context = new Model())
                {
                    var addStore = new Stores
                    {
                        StoreName = request.DisplayName,
                        AddressCh = request.Address,
                        AddressEn = addressEn,
                        Phone = phone,
                        StoreUrl = storeUrl,
                        StoreGoogleId = request.PlaceId,
                        BusinessHours = businessHours,
                        XLocation = request.Location.Lat,
                        YLocation = request.Location.Lng,
                        StoreTags = request.Tags != null ? string.Join(",", request.Tags) : string.Empty,
                        CreateTime = DateTime.Now,
                    };
                    context.Stores.Add(addStore);
                    context.SaveChanges();  // 儲存變更

                    var result = new
                    {
                        statusCode = 200,
                        status = true,
                        message = "新增店家成功",
                        storeId = addStore.Id
                    };
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                // 錯誤處理
                var errorResponse = new
                {
                    statusCode = 500,
                    status = false,
                    message = "伺服器錯誤",
                    error = ex.Message
                };
                return InternalServerError(new Exception("詳細錯誤訊息", ex));
            }
        }


        // PUT: api/Stores/5
        public void Put([FromBody] string value)
        {

        }

        // DELETE: api/Stores/5
        public void Delete(int id)
        {
        }
        [HttpGet]
        [Route("api/allstores")]
        public IHttpActionResult GetAllStores()
        {
            try
            {
                using (var context = new Model())
                {
                    // 查詢所有店家
                    var stores = context.Stores.ToList();
                    var result = stores.Select(store => new
                    {
                        store.StoreName,
                        store.AddressCh,
                        store.AddressEn,
                        store.PriceStart,
                        store.PriceEnd,
                        store.Phone,
                        store.ReserveUrl,
                        store.BusinessHours,
                        store.XLocation,
                        store.YLocation,
                    });
                    if (stores == null || stores.Count == 0)
                    {
                        return NotFound(); // 如果沒有店家資料，返回 404
                    }
                    return Ok(
                        new
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
        [HttpGet]
        [Route("api/stores/{storeid}")]
        public IHttpActionResult GetStores(int storeid)
        {
            try
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
                        var response = new
                        {
                            statusCode = 401,
                            status = false,
                            message = "Token 無效或已過期"
                        };
                        return Content(System.Net.HttpStatusCode.Unauthorized, response);
                    }
                }
                using (var context = new Model())
                {
                    var store = context.Stores.FirstOrDefault(m => m.Id == storeid);
                    var mid = context.CollectStore.Where(c => c.StoreId == storeid & c.MemberId == memberId);
                    if (store != null)
                    {
                        var storeStars = context.StoreComments.Where(m => m.StoreId == storeid).ToList();
                        var comments = context.StoreComments.ToList();
                        var search = context.SearchCondition.ToList();
                        string idr = context.Configs.FirstOrDefault(i => i.Group == "IDR").MVal;
                        int IDR = int.Parse(idr);
                        var averageStars = 0;
                        if (storeStars.Count > 0)
                        {
                            averageStars = (int)Math.Round(storeStars.Average(m => m.Stars));
                        }
                        var openingHours = JsonConvert.DeserializeObject<Dictionary<string, string>>(store.BusinessHours);
                        var tags = comments.Where(c => !string.IsNullOrEmpty(c.Label)).GroupBy(c => c.StoreId)
                                    .Select(g => new
                                    {
                                        Id = g.Key,
                                        Tags = string.Join(",", g.SelectMany(c => c.Label.Split(',').Select(label => label.Trim())).Distinct())
                                    })
                                    .ToList();
                        var data = new
                        {
                            advertise = new
                            {
                                photos = "",
                                url = "",
                                title = "",
                                slogan = ""
                            },
                            id = store.Id,
                            starCount = averageStars,
                            tags = comments
                            .Where(c => !string.IsNullOrEmpty(c.Label) && c.StoreId == store.Id)
                            .SelectMany(c => c.Label.Split(','))
                            .GroupBy(label => label.Trim())
                            .Select(group =>
                            {
                                var searchItem = search.FirstOrDefault(s => s.Id.ToString() == group.Key);
                                return new
                                {
                                    tagName = searchItem.MVal,
                                    count = group.Count()
                                };
                            })
                            .ToList(),
                            isFavorited = mid.Count() > 0 ? true : false,
                            placeId = store.StoreGoogleId,
                            location = new { lat = store.XLocation, lng = store.YLocation },
                            displayName = store.StoreName,
                            photos = store.StorePictures,
                            address = store.AddressCh,
                            enAddress = store.AddressEn,
                            book = store.ReserveUrl,
                            budget = "NTD " + store.PriceStart + "~" + store.PriceEnd + " / Rp " + store.PriceStart * IDR + "~" + store.PriceEnd * IDR,
                            phone = store.Phone,
                            url = "",
                            opening_hours = openingHours
                        };
                        var response = new
                        {
                            statusCode = 200,
                            status = true,
                            message = "資料取得成功",
                            data
                        };
                        return Ok(response);
                    }
                    else
                    {
                        var response = new
                        {
                            statusCode = 200,
                            status = true,
                            message = "查無店家資料",
                        };
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
        [HttpPost]
        [Route("api/stores/search")]
        public IHttpActionResult SearchStores([FromBody] SearchRequest request)
        {
            try
            {
                var req = Request;
                int? memberId = null;
                if (req.Headers.Authorization == null || req.Headers.Authorization.Scheme != "Bearer")
                {
                    memberId = 0;
                }
                else
                {
                    try
                    {
                        var jwtObject = JwtAuthUtil.GetToken(req.Headers.Authorization.Parameter);
                        memberId = int.Parse(jwtObject["Id"].ToString());
                    }
                    catch
                    {
                        var response = new
                        {
                            statusCode = 401,
                            status = false,
                            message = "Token 無效或已過期"
                        };
                        return Content(System.Net.HttpStatusCode.Unauthorized, response);
                    }
                }
                using (var context = new Model())
                {
                    const double radiusInKm = 3;
                    var locationType = request.LocationType;
                    var cityId = request.CityId;
                    var cityName = context.City.FirstOrDefault(m => m.Id == cityId)?.CountryName ?? string.Empty;
                    string tag = string.Join(",", request.Tags);
                    var comments = context.StoreComments.Include(c => c.Members).Include(c => c.CommentLikes).ToList();
                    var replies = context.Reply.Include(r => r.Members).Include(r => r.StoreComments).ToList();
                    var replyLike = context.ReplyLike.ToList();
                    var stores = context.Stores.Include(s => s.StoreComments).ToList();
                    var collects = context.CollectStore.ToList();
                    var search = context.SearchCondition.ToList();
                    var storetag = comments.Where(c => !string.IsNullOrEmpty(c.Label)).GroupBy(c => c.StoreId)
                                    .Select(g => new
                                    {
                                        Id = g.Key,
                                        Tags = string.Join(",", g.SelectMany(c => c.Label.Split(',').Select(label => label.Trim())).Distinct())
                                    })
                                    .ToList();
                    double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
                    {
                        const double earthRadius = 6371.0;
                        double dLat = (lat2 - lat1) * (Math.PI / 180.0);
                        double dLon = (lon2 - lon1) * (Math.PI / 180.0);

                        lat1 *= Math.PI / 180.0;
                        lat2 *= Math.PI / 180.0;

                        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
                        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                        return earthRadius * c;
                    }

                    if (locationType == "user")
                    {
                        var latitude = request.Location.Lat;
                        var longitude = request.Location.Lng;
                        stores = stores.Where(s => CalculateDistance(latitude, longitude, (double)s.XLocation, (double)s.YLocation) <= radiusInKm).ToList();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(tag))
                        {
                            var filteredTags = storetag.Where(st => st.Tags.Contains(tag)).Select(st => st.Id).ToList();
                            stores = stores.Where(s => filteredTags.Contains(s.Id)).ToList();
                        }
                        if (!string.IsNullOrEmpty(cityName))
                        {
                            stores = stores.Where(store => store.AddressCh != null && store.AddressCh.Contains(cityName)).ToList();
                        }

                    }
                    if (stores.Count > 0)
                    {
                        var data = stores.Select(store => new
                        {
                            id = store.Id,
                            placeId = store.StoreGoogleId,
                            displayName = store.StoreName,
                            photos = store.StorePictures,
                            starCount = (int)Math.Round(comments.Where(m => m.StoreId == store.Id)
                                        .Select(m => m.Stars).DefaultIfEmpty(0)
                                        .Average(), MidpointRounding.AwayFromZero),
                            isAdvertise = store.IsAdvertise,
                            isFavorited = collects.Any(c => c.StoreId == store.Id && c.MemberId == memberId),
                            reviewCount = comments.Where(c => c.StoreId == store.Id).Count(),
                            replyCount = replies.Where(r => r.StoreComments.StoreId == store.Id).Count(),
                            tags = comments
                            .Where(c => !string.IsNullOrEmpty(c.Label) && c.StoreId == store.Id)
                            .SelectMany(c => c.Label.Split(','))
                            .GroupBy(label => label.Trim())
                            .Select(group =>
                            {
                                var searchItem = search.FirstOrDefault(s => s.Id.ToString() == group.Key);
                                return new
                                {
                                    tagName = searchItem.MVal,
                                    count = group.Count()
                                };
                            })
                            .ToList(),
                            comments = comments.Where(c => c.StoreId == store.Id).Select(c => new
                            {
                                commentId = c.Id,
                                userPhoto = c.Members.Photo,
                                content = c.Comment,
                            })
                        }).ToList();
                        var result = new
                        {
                            statusCode = 200,
                            status = true,
                            message = "取得店家列表成功",
                            data
                        };

                        return Ok(result);
                    }
                    else
                    {
                        var randomStores = context.Stores.OrderBy(s => Guid.NewGuid()).Take(5).ToList();
                        var data = randomStores.Select(store => new
                        {
                            id = store.Id,
                            placeId = store.StoreGoogleId,
                            displayName = store.StoreName,
                            photos = store.StorePictures,
                            starCount = (int)Math.Round(comments.Where(m => m.StoreId == store.Id)
                                        .Select(m => m.Stars).DefaultIfEmpty(0)
                                        .Average(), MidpointRounding.AwayFromZero),

                            isAdvertise = store.IsAdvertise,
                            isFavorited = collects.Any(c => c.StoreId == store.Id && c.MemberId == memberId),
                            reviewCount = comments.Where(c => c.StoreId == store.Id).Count(),
                            replyCount = replies.Where(r => r.StoreComments.StoreId == store.Id).Count(),
                            tags = comments
                            .Where(c => !string.IsNullOrEmpty(c.Label) && c.StoreId == store.Id)
                            .SelectMany(c => c.Label.Split(','))
                            .GroupBy(label => label.Trim())
                            .Select(group =>
                            {
                                var searchItem = search.FirstOrDefault(s => s.Id.ToString() == group.Key);
                                return new
                                {
                                    tagName = searchItem.MVal,
                                    count = group.Count()
                                };
                            })
                            .ToList(),
                            comments = comments.Where(c => c.StoreId == store.Id).Select(c => new
                            {
                                commentId = c.Id,
                                userPhoto = c.Members.Photo,
                                content = c.Comment,
                            })
                        }).ToList();
                        var result = new
                        {
                            statusCode = 200,
                            status = true,
                            message = "沒有店家",
                            data
                        };

                        return Ok(result);
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
        [Route("api/storesstars/{id}")]
        public IHttpActionResult GetStoresStars(int id)
        {
            try
            {
                using (var context = new Model())
                {
                    var storeStars = context.StoreComments.Where(m => m.StoreId == id).ToList();
                    if (!storeStars.Any())
                    {
                        return Ok(new
                        {
                            AverageStars = 0,
                        });
                    }
                    else
                    {
                        var averageStars = (int)Math.Round(storeStars.Average(m => m.Stars));
                        return Ok(new
                        {
                            AverageStars = averageStars
                        });
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
        [Route("collectshop/get")]
        public IHttpActionResult collectshop()
        {
            try
            {
                var request = Request;
                int? memberId = null;
                if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Bearer")
                {
                    var response = new
                    {
                        statusCode = 200,
                        status = true,
                        message = "尚未登入",
                    };
                    return Ok(response);
                }
                try
                {
                    var jwtObject = JwtAuthUtil.GetToken(request.Headers.Authorization.Parameter);
                    memberId = int.Parse(jwtObject["Id"].ToString());
                }
                catch
                {
                    var response = new
                    {
                        statusCode = 401,
                        status = false,
                        message = "Token 無效或已過期"
                    };
                    return Content(System.Net.HttpStatusCode.Unauthorized, response);
                }
                var collectstores = db.CollectStore.Where(c => c.MemberId == memberId).Include(c => c.Stores).ToList();
                var comments = db.StoreComments.ToList();
                var replies = db.Reply.ToList();
                var search = db.SearchCondition.ToList();
                var data = collectstores.Select(cs => new
                {
                    id = cs.Stores.Id,
                    displayName = cs.Stores.StoreName,
                    photos = cs.Stores.StorePictures,
                    starCount = cs.Stores.StoreComments
                                            .Where(m => m.StoreId == cs.Id)
                                            .Select(m => (double?)m.Stars)
                                            .DefaultIfEmpty(0)
                                            .Average(),
                    isAdvertise = cs.Stores.IsAdvertise,
                    isFavorited = cs.Stores.CollectStores.Any(c => c.StoreId == cs.Id && c.MemberId == memberId),
                    reviewCount = cs.Stores.StoreComments.Where(c => c.StoreId == cs.Id).Count(),
                    repliesCount = replies.Where(r => r.StoreComments.StoreId == cs.Id).Count(),
                    tags = comments
                            .Where(c => !string.IsNullOrEmpty(c.Label) && c.StoreId == cs.StoreId)
                            .SelectMany(c => c.Label.Split(','))
                            .GroupBy(label => label.Trim())
                            .Select(group =>
                            {
                                var searchItem = search.FirstOrDefault(s => s.Id.ToString() == group.Key);
                                return new
                                {
                                    tagName = searchItem.MVal,
                                    count = group.Count()
                                };
                            })
                            .ToList(),

                    comments = cs.Stores.StoreComments.Where(c => c.StoreId == cs.StoreId).Select(c => new
                    {
                        commentId = c.Id,
                        userPhoto = c.Members.Photo,
                        content = c.Comment,
                    })
                }).ToList();

                if (data == null || data.Count == 0)
                {
                    var response = new
                    {
                        statusCode = 200,
                        status = true,
                        message = "用戶無收藏店家",
                    };
                    return Ok(response);
                }
                else
                {
                    var response = new
                    {
                        statusCode = 200,
                        status = true,
                        message = "資料取得成功",
                        data
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                // 捕獲異常並返回具體錯誤訊息
                return InternalServerError(new Exception("詳細錯誤訊息", ex));
            }
        }
        [HttpGet]
        [Route("api/storescomments/{id}")]
        public IHttpActionResult GetStoresComments(int id)
        {
            try
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
                        var response = new
                        {
                            statusCode = 401,
                            status = false,
                            message = "Token 無效或已過期"
                        };
                        return Content(System.Net.HttpStatusCode.Unauthorized, response);
                    }
                }
                using (var context = new Model())
                {
                    var storeComments = context.StoreComments.Where(m => m.StoreId == id).Include(m => m.Members).Include(m => m.CommentPictures).Include(m => m.CommentLikes).ToList();
                    var searchCondition = context.SearchCondition.ToList();
                    var data = storeComments.Select(comment => new
                    {
                        commentId = comment.Id,
                        userId = comment.MemberId,
                        userName = comment.Members.Name,
                        userPhoto = comment.Members.Photo,
                        country = comment.Members.Country,
                        comment = comment.Comment,
                        photos = comment.CommentPictures,
                        replyCount = storeComments.Count(),
                        starCount = (int)Math.Round(storeComments.Select(m => m.Stars).DefaultIfEmpty(0).Average(), MidpointRounding.AwayFromZero),
                        createTime = comment.CreateTime.ToString(),
                        likeCount = storeComments.Count(),
                        isLike = comment.CommentLikes.Any(sc => sc.LikeUserId == memberId) ? true : false,
                        tags = searchCondition.Where(condition => comment.Label.Split(',')
                        .Select(tag => int.Parse(tag.Trim())).ToList()
                        .Contains(condition.Id)).Select(condition => condition.MVal)
                        .ToList(),
                        badge = comment.Members.Badge,
                    });
                    var result = new
                    {
                        statusCode = 200,
                        status = true,
                        message = "資料取得成功",
                        data
                    };
                    if (!storeComments.Any())
                    {
                        result = new
                        {
                            statusCode = 200,
                            status = true,
                            message = "無評論",
                            data
                        };
                        return Ok(result);
                    }
                    else
                    {

                        return Ok(result);
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
        [Route("api/stores/tops")]
        public IHttpActionResult Gettopstores()
        {
            var topComments = db.Stores
                .OrderByDescending(store => store.Engagement)
                .Take(4)
                .Select(store => new
                {
                    Store = store,
                    Comment = db.StoreComments
                    .Where(comment => comment.StoreId == store.Id)
                    .OrderByDescending(comment => db.CommentLike
                    .Count(like => like.CommentId == comment.Id))
                    .FirstOrDefault(),
                    AverageStars = db.StoreComments
                    .Where(comment => comment.StoreId == store.Id)
                    .Select(comment => comment.Stars)
                    .DefaultIfEmpty(0)
                    .Average()
                })
                .Select(x => new
                {
                    Store = x.Store,
                    Comment = x.Comment != null ? x.Comment : null,
                    MemberPhoto = x.Comment != null
                    ? db.Member
                    .Where(member => member.Id == x.Comment.MemberId)
                    .Select(member => member.Photo)
                    .FirstOrDefault()
                    : null,
                    Sp = db.StorePictures
                    .Where(StorePictures => StorePictures.StoreId == x.Store.Id)
                    .Select(StorePictures => StorePictures.PictureUrl)
                    .FirstOrDefault(),
                    AverageStars = x.AverageStars
                })
                .ToList();

            var dataList = new List<object>();
            List<string> storelist = new List<string>();

            string userPath = ConfigurationManager.AppSettings["UserPhoto"];
            string storePath = ConfigurationManager.AppSettings["StorePhoto"];

            foreach (var item in topComments)
            {
                string[] storeSavePath = item.Sp == null ? new string[] { } : new string[] { Path.Combine(storePath, item.Sp) };
                string userSavePath = item.MemberPhoto == null ? null : Path.Combine(userPath, item.MemberPhoto);

                var commentData = item.Comment != null ? new
                {
                    commitId = item.Comment.Id,
                    userPhoto = userSavePath,
                    content = item.Comment.Comment
                }
                : null;


                var datadictionary = new
                {
                    placeId = item.Store.StoreGoogleId,
                    displayName = item.Store.StoreName,
                    photos = storeSavePath,
                    starCount = item.AverageStars == 0 ? 0 : item.AverageStars,
                    comment = commentData
                };
                dataList.Add(datadictionary);
            }

            return Ok(new
            {
                statusCode = 200,
                status = true,
                message = "隨機熱門店家搜尋成功!",
                data = dataList
            });
        }

        public class SearchRequest
        {
            public int memberID { get; set; }
            public string LocationType { get; set; }
            public Location Location { get; set; }
            public int CityId { get; set; }
            public List<int> Tags { get; set; }
        }

        public class Location
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
        public class DecimalLocation
        {
            public decimal Lat { get; set; }
            public decimal Lng { get; set; }
        }
        public class AddRequest
        {
            public DecimalLocation Location { get; set; }
            public List<string> Photos { get; set; }
            public string PlaceId { get; set; }
            public string DisplayName { get; set; }
            public string Address { get; set; }
            public List<int> Tags { get; set; }
        }
    }
}
