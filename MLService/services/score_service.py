from price_rules import check_price_validity

def calculate_score(data):
    description = data.description
    image_count = data.imageCount
    price = data.price
    category = data.category

    description_length = len(description)
    score = 0

    # =========================
    # 📊 BASIC SCORING
    # =========================
    if description_length > 50:
        score += 30
    else:
        score += 10

    if image_count >= 2:
        score += 30
    elif image_count == 1:
        score += 10

    if category and category != "Unknown":
        score += 20

    # =========================
    # 🔥 PRICE VALIDATION
    # =========================
    valid_price, reason = check_price_validity(description, category, price)

    if valid_price is True:
        score += 20

    elif valid_price is False:
        # 🚫 strong penalty
        score -= 50

        # 🔥 force downgrade
        if score > 40:
            score = 40

    elif valid_price is None:
        # ⚠️ unknown product → mild penalty
        score -= 20

    # =========================
    # 🎯 FINAL STATUS
    # =========================
    if score < 40:
        status = "Poor"
    elif score <= 70:
        status = "Average"
    else:
        status = "Good"

    # =========================
    # 📦 RESPONSE
    # =========================
    result = {
        "score": max(score, 0),
        "status": status
    }

    # 🔥 Add messages properly
    if valid_price is False:
        result["price_issue"] = reason

    elif valid_price is None:
        result["warning"] = reason

    else:
        result["price_valid"] = True

    return result