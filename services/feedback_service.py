from price_rules import check_price_validity

def generate_feedback(data):
    description = data.description
    image_count = data.imageCount
    price = data.price
    category = data.category

    issues = []

    if len(description) < 50:
        issues.append("Improve description")

    if image_count < 2:
        issues.append("Add more images")

    if not category:
        issues.append("Select valid category")

    valid, reason = check_price_validity(description, category, price)

    if not valid:
        issues.append(reason)

    suggestion = "Please fix the above issues" if issues else "Product looks good"

    return {"issues": issues, "suggestion": suggestion}