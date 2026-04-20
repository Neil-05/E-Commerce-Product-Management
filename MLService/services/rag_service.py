from sentence_transformers import SentenceTransformer
from sklearn.metrics.pairwise import cosine_similarity
from model_loader import load_models


_,_, embedder= load_models()
# Simple knowledge base
docs = [

    # =========================
    # 📱 ELECTRONICS
    # =========================
    "Smartphones should be priced between 3000 and 160000 depending on brand and features",
    "Televisions should be priced between 20000 and 500000 depending on size and technology like OLED or LED",
    "Laptops should be priced between 20000 and 200000 based on performance and specifications",
    "Electronics products must have at least 2-3 images showing different angles",
    "Electronics descriptions should include specifications like battery, processor, display and features",
    "Very low price for electronics may indicate poor quality or counterfeit product",

    # =========================
    # 📚 BOOKS
    # =========================
    "Books are usually priced between 200 and 4000 depending on type and author",
    "Academic and technical books can be priced higher than regular novels",
    "Books should include description about author, summary and edition",
    "Books generally require at least 1 clear image of the cover",

    # =========================
    # 👕 CLOTHING
    # =========================
    "Clothing products are usually priced between 300 and 10000 depending on brand and quality",
    "Clothing items should include size, fabric type, and fit details in description",
    "Clothing listings should have multiple images showing front, back and usage",
    "Very cheap clothing may indicate low quality fabric",

    # =========================
    # ⚽ SPORTS
    # =========================
    "Sports products typically range between 500 and 50000 depending on type and brand",
    "Sports items should include usage details and durability information",
    "Images should clearly show the product in action or usage",

    # =========================
    # 💄 BEAUTY
    # =========================
    "Beauty and personal care products are priced between 200 and 20000 depending on brand",
    "Beauty products must include ingredients and usage instructions",
    "Images should clearly show packaging and product",

    # =========================
    # 🛒 GROCERY
    # =========================
    "Grocery products are typically priced between 50 and 5000 depending on quantity and brand",
    "Grocery items must include expiry date and quantity details",
    "Images should clearly show packaging and label",

    # =========================
    # 🧠 GENERAL RULES
    # =========================
    "Products should have at least 2 images for better quality listing",
    "Product descriptions should be detailed and more than 50 characters",
    "Extremely low pricing compared to category standards may indicate invalid or suspicious listing",
    "High quality listings improve approval chances significantly"
]

doc_embeddings = None  # 🔥 lazy cache


def get_doc_embeddings():
    global doc_embeddings

    if doc_embeddings is None:
        doc_embeddings = embedder.encode(docs)

    return doc_embeddings


def retrieve_context(query):
    embeddings = get_doc_embeddings()

    query_embedding = embedder.encode([query])

    scores = cosine_similarity(query_embedding, embeddings).flatten()
    idx = scores.argmax()

    return docs[idx]