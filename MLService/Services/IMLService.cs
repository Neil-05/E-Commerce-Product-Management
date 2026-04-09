using global::MLService.DTOs;
using MLService.DTOs;
namespace MLService.Services
{


   

    public interface IMLService
    {
        int CalculateCompletenessScore(ProductInputDto dto);
    }
}
