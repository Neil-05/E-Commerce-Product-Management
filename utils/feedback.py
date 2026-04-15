def generate_feedback(data: dict) -> dict:
    """
    Generate actionable feedback based on product attributes.
    Rules:
    - description length < 50 -> "Improve description"
    - imageCount < 2 -> "Add more images"
    - price <= 500 -> "Check pricing"
    - invalid category -> "Select valid category"
    
    data: dict containing:
        - description: string
        - imageCount: int
        - price: float
        - category: string
    """
    issues = []
    
    description = data.get("description", "")
    description_length = len(description) if description else 0
    
    if description_length < 50:
        issues.append("Improve description")
        
    image_count = data.get("imageCount", 0)
    if image_count < 2:
        issues.append("Add more images")
        
    price = data.get("price", 0.0)
    if price <= 500:
        issues.append("Check pricing")
        
    category = data.get("category", "")
    if not category or category.lower() == "unknown":
        issues.append("Select valid category")
        
    suggestion = "Product is well configured."
    if len(issues) > 0:
        suggestion = "Please address the listed issues to improve your product listing."
        
    return {
        "issues": issues,
        "suggestion": suggestion
    }
