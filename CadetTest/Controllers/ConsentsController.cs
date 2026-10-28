using CadetTest.Models;
using CadetTest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CadetTest.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConsentsController : ControllerBase
    {
        private IDataService _dataService;

        public ConsentsController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpPost]
        public IActionResult Post(ConsentRequest request)
        {
            var cevap = _dataService.GetRangeById(request.StartId, request.Count);
            return Ok(cevap);
        }
    }
}
