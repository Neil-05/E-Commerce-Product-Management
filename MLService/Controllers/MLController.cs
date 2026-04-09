using global::MLService.DTOs;
using global::MLService.Services;
using Microsoft.AspNetCore.Mvc;
using MLService.DTOs;
using MLService.Services;
namespace MLService.Controllers
{
   

    [ApiController]
    [Route("api/ml")]
    public class MLController : ControllerBase
    {
        private readonly IMLService _mlService;

        public MLController(IMLService mlService)
        {
            _mlService = mlService;
        }

        [HttpPost("score-product")]
        public IActionResult ScoreProduct(ProductInputDto dto)
        {
            var score = _mlService.CalculateCompletenessScore(dto);

            return Ok(new
            {
                score = score,
                status = score > 70 ? "Good" : "Needs Improvement"
            });
        }
    }
}
