from services.models import ProductRequest
import re
from sentence_transformers import SentenceTransformer
from sklearn.metrics.pairwise import cosine_similarity
from model_loader import load_models

# 🔥 Load model once
_,_,embedder=load_models()

# 🔥 Category vocabulary (semantic)
CATEGORY_MAP = {

    # =========================
    # 📱 ELECTRONICS
    # =========================
    "Electronics": [
        # Mobile & devices
        "phone", "smartphone", "mobile", "iphone", "android", "tablet", "ipad",

        # Computers
        "laptop", "notebook", "computer", "pc", "desktop", "macbook",

        # TV & media
        "tv", "television", "smart tv", "led tv", "oled tv",

        # Gaming
        "playstation", "ps", "ps4", "ps5", "xbox", "console", "gaming console",

        # Accessories
        "headphones", "earphones", "earbuds", "airpods", "speaker", "bluetooth speaker",
        "charger", "cable", "power bank",

        # Cameras
        "camera", "dslr", "mirrorless camera", "webcam",

        # Home electronics
        "refrigerator", "fridge", "washing machine", "microwave", "oven",
        "air conditioner", "ac", "cooler", "fan"
    ],

    # =========================
    # 📚 BOOKS
    # =========================
    "Books": [
        "book", "novel", "story book", "magazine", "journal",
        "textbook", "study material", "guide", "comic", "ebook"
    ],

    # =========================
    # 👕 CLOTHING
    # =========================
    "Clothing": [
        "shirt", "tshirt", "t-shirt", "jeans", "pant", "trouser",
        "dress", "kurta", "saree", "jacket", "coat", "hoodie",
        "shorts", "skirt", "blazer", "clothes", "outfit"
    ],

    # =========================
    # ⚽ SPORTS
    # =========================
    "Sports": [
        "football", "cricket", "bat", "ball", "tennis", "racket",
        "badminton", "basketball", "volleyball", "sports gear",
        "dumbbell", "gym equipment", "treadmill", "yoga mat"
    ],

    # =========================
    # 💄 BEAUTY
    # =========================
    "Beauty & Personal Care": [
        "cream", "face cream", "moisturizer", "cosmetic", "makeup",
        "lipstick", "foundation", "skincare", "facewash", "shampoo",
        "conditioner", "perfume", "deodorant", "soap"
    ],

    # =========================
    # 🛒 GROCERY
    # =========================
    "Grocery": [
        "rice", "wheat", "atta", "flour", "dal", "lentils",
        "oil", "sugar", "salt", "spices", "tea", "coffee",
        "snacks", "biscuits", "food", "vegetables", "fruits"
    ],

    # =========================
    # 🏠 HOME & KITCHEN
    # =========================
    "Home & Kitchen": [
        "utensils", "cookware", "pan", "kadhai", "pressure cooker",
        "knife", "kitchen tools", "furniture", "chair", "table",
        "bed", "sofa", "mattress", "home decor"
    ],

    # =========================
    # 🚗 AUTOMOTIVE
    # =========================
    "Automotive": [
        "car", "bike", "motorcycle", "helmet", "car accessories",
        "engine oil", "tyre", "battery", "vehicle parts"
    ],

    # =========================
    # 🧸 TOYS
    # =========================
    "Toys & Games": [
        "toy", "toy car", "doll", "board game", "lego",
        "puzzle", "video game", "gaming"
    ]
}

# 🔥 Precompute embeddings
category_embeddings = None  # lazy cache


def get_category_embeddings():
    global category_embeddings

    if category_embeddings is None:
        category_embeddings = {
            cat: embedder.encode(words)
            for cat, words in CATEGORY_MAP.items()
        }

    return category_embeddings


def detect_category_semantic(message):
    embeddings = get_category_embeddings()
    query_embedding = embedder.encode([message])

    best_category = None
    best_score = 0

    for category, embeddings in category_embeddings.items():
        scores = cosine_similarity(query_embedding, embeddings).flatten()
        max_score = scores.max()

        if max_score > best_score:
            best_score = max_score
            best_category = category

    # 🔥 UNKNOWN RULE
    if best_score < 0.4:
        return "Unknown"

    return best_category


def parse_product_from_text(message: str):
    message = message.lower()

    # 🔢 Extract price
    price_match = re.search(r"\d+", message)
    price = int(price_match.group()) if price_match else 0

    # 🖼️ Extract images
    image_match = re.search(r"(\d+)\s*image", message)
    image_count = int(image_match.group(1)) if image_match else 1

    # 🧠 Semantic category detection
    # =========================
    # 🔥 RULE-BASED MATCH FIRST
    # =========================
    category = "Unknown"

    for cat, keywords in CATEGORY_MAP.items():
        for word in keywords:
            pattern = r'\b' + re.escape(word) + r'\b'
            if re.search(pattern, message):
                category = cat
                break
        if category != "Unknown":
            break

    # =========================
    # 🤖 FALLBACK TO SEMANTIC
    # =========================
    if category == "Unknown":
        category = detect_category_semantic(message)
    
    # print("category", category)

    return ProductRequest(
        description=message,
        imageCount=image_count,
        price=price,
        category=category
    )