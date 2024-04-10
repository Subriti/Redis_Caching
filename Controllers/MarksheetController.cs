using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace Redis_Caching.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarksheetController : ControllerBase
    {
        private readonly ConnectionMultiplexer _redisConnection;

        public MarksheetController(ConnectionMultiplexer redisConnection)
        {
            _redisConnection = redisConnection;
        }

        // Pre-defined array list of marksheets
        private readonly List<Dictionary<string, object>> predefinedMarksheets = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "user_id", 1 }, { "marks", new Dictionary<string, int> { { "math", 90 }, { "science", 85 }, { "english", 92 } } } },
            new Dictionary<string, object> { { "user_id", 2 }, { "marks", new Dictionary<string, int> { { "math", 80 }, { "science", 75 }, { "english", 85 } } } },
            new Dictionary<string, object> { { "user_id", 3 }, { "marks", new Dictionary<string, int> { { "math", 95 }, { "science", 90 }, { "english", 88 } } } }
        };

        private List<Dictionary<string, object>> GetMarksheets()
        {
            var db = _redisConnection.GetDatabase();
            var marksheets = db.StringGet("marksheets");
            if (!marksheets.IsNull)
            {
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(marksheets);
            }
            else
            {
                db.StringSet("marksheets", JsonSerializer.Serialize(predefinedMarksheets));
                return predefinedMarksheets;
            }
        }

        [HttpGet("{userId}")]
        public IActionResult GetMarksheet(int userId)
        {
            var marksheets = GetMarksheets();
            foreach (var marksheet in marksheets)
            {
                if (int.TryParse(marksheet["user_id"].ToString(), out int userIdFromSheet) && userIdFromSheet == userId)
                {
                    return Ok(marksheet);
                }
            }
            return NotFound(new { error = "Student not found" });
        }
    }
}
