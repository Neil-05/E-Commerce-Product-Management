def calculate_score(data: dict) -> dict:
    """
    Calculate completeness score based on rules:
    - description_length > 50 -> +30
    - imageCount >= 2 -> +30
    - price > 500 -> +20
    - valid category -> +20
    
    data: dict containing:
        - description: string (the full description, we will calculate length)
        - imageCount: int
        - price: float
        - category: string
    """
    score = 0
    
    description = data.get("description", "")
    description_length = len(description) if description else 0
    
    if description_length > 50:
        score += 30
        
    image_count = data.get("imageCount", 0)
    if image_count >= 2:
        score += 30
        
    price = data.get("price", 0.0)
    if price > 500:
        score += 20
        
    category = data.get("category", "")
    # Treat empty, "Unknown" or None as invalid category
    if category and category.lower() != "unknown":
        score += 20
        
    status = "Good"
    if score < 60:
        status = "Poor"
    elif score < 80:
        status = "Average"
        
    return {
        "score": score,
        "status": status
    }
