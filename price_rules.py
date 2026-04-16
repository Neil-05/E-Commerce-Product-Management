PRICE_RULES = {
    "Electronics": {
        "phone": (3000, 160000),
        "tv": (20000, 500000),
        "laptop": (20000, 200000),
        "console":(30000, 90000),
        "general": (1000, 300000)
    },
    "Books": {
        "novel": (200, 2000),
        "textbook": (300, 5000),
        "magazine": (100, 1000),
        "general": (200, 4000)
    },
    "Clothing": {
        "shirt": (300, 5000),
        "jeans": (500, 8000),
        "general": (300, 10000)
    },
    "Sports": {
        "football": (500, 3000),
        "equipment": (1000, 50000),
        "general": (500, 50000)
    },
    "Beauty & Personal Care": {
        "cream": (200, 5000),
        "general": (200, 20000)
    },
    "Grocery": {
        "food": (50, 2000),
        "general": (50, 5000)
    }
}

KEYWORD_MAP = {
    "phone": ["phone", "mobile", "smartphone", "iphone", "android"],
    "tv": ["tv", "television", "smart tv"],
    "laptop": ["laptop", "notebook", "computer"],
  
    "console": ["ps", "ps4", "ps5", "xbox", "playstation","nintendo"],


    "novel": ["novel", "story", "fiction"],
    "textbook": ["textbook", "study material"],
    "magazine": ["magazine", "journal"],

    "shirt": ["shirt", "tshirt", "t-shirt"],
    "jeans": ["jeans", "denim"],

    "football": ["football"],
    "equipment": ["gym", "fitness", "dumbbell", "equipment"],

    "cream": ["cream", "moisturizer"],
    "food": ["rice", "snacks", "food", "coffee", "tea"]
}


def check_price_validity(description, category, price):

    if not category or category == "Unknown":
        return None, "Product type is missing (e.g., phone, TV, book)"

    category = category.strip().title()
    rules = PRICE_RULES.get(category)

    if not rules:
        return None, f"No pricing rules defined for category '{category}'"

    desc = description.lower()

    matched_product = None
    best_score = 0

    # =========================
    # 🔎 SMART MATCHING (score-based)
    # =========================
    for keyword, (min_p, max_p) in rules.items():

        if keyword == "general":
            continue

        score = 0

        for word in KEYWORD_MAP.get(keyword, [keyword]):
            if f" {word} " in f" {desc} ":
                score += 1

        if score > best_score:
            best_score = score
            matched_product = keyword

    # =========================
    # 🎯 IF SPECIFIC MATCH FOUND
    # =========================
    print("Matched product:", matched_product)
    print("Price:", price)
    print("Rule:", rules.get(matched_product))
    if matched_product:
        min_p, max_p = rules[matched_product]

        if price < min_p or price > max_p:
            return False, f"{matched_product} price should be between {min_p} and {max_p}"

        return True, None

    # =========================
    # 🔥 FALLBACK TO GENERAL
    # =========================
    if "general" in rules:
        min_p, max_p = rules["general"]

        if price < min_p or price > max_p:
            return False, f"{category} products usually range between {min_p} and {max_p}"

        return True, None   # ✅ NO ERROR, ACCEPTABLE

    return None, "Product type unclear"