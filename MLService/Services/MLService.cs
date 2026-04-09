using global::MLService.DTOs;
using MLService.DTOs;

namespace MLService.Services
{
    
  

    public class MLService : IMLService
    {
        public int CalculateCompletenessScore(ProductInputDto dto)
        {
            int score = 0;

            // Name check
            if (!string.IsNullOrWhiteSpace(dto.Name))
                score += 20;

            // Description quality
            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                if (dto.Description.Length > 50)
                    score += 25;
                else
                    score += 10;
            }

            // Price
            if (dto.Price > 0)
                score += 15;

            // Images
            if (dto.ImageCount >= 3)
                score += 20;
            else if (dto.ImageCount > 0)
                score += 10;

            // Category
            if (!string.IsNullOrWhiteSpace(dto.Category))
                score += 20;

            return score;
        }
    }
}
