from sentence_transformers import SentenceTransformer
from sklearn.metrics.pairwise import cosine_similarity
from model_loader import load_models

_,_,embedder=load_models()
# 🔥 INTENTS
INTENTS = {

    # =========================
    # 🔮 PREDICT API
    # =========================
    "predict": [
        "predict product approval",
        "will this product be approved",
        "approval prediction",
        "is this likely to be approved",
        "predict approval",
        "will this pass approval",
        "approval chance",
        "probability of approval"
    ],

    # =========================
    # 🎯 VALIDATE API (MAIN DECISION)
    # =========================
    "validate": [
        "check this product",
        "validate this product",
        "is this product good",
        "is this okay to sell",
        "should I list this",
        "can I upload this product",
        "is this valid listing",
        "review this product",
        "check product quality",
        "is this acceptable"
    ],

    # =========================
    # 📊 SCORE API
    # =========================
    "score": [
        "give score",
        "what is the score",
        "rate this product",
        "product quality score",
        "how good is this product",
        "evaluate product quality",
        "score this item",
        "give rating"
    ],

    # =========================
    # 💬 FEEDBACK API
    # =========================
    "feedback": [
        "what is wrong with this",
        "issues with product",
        "problems in listing",
        "what should I fix",
        "give feedback",
        "why is this bad",
        "what are the mistakes",
        "how to improve this product",
        "what is missing"
    ],

    # =========================
    # 🤖 RECOMMEND API
    # =========================
    "recommend": [
        "recommend products",
        "suggest products",
        "show me products",
        "what should I buy",
        "suggest items for me",
        "find similar products",
        "recommend something",
        "show me options",
        "suggest sports items",
        "recommend electronics",
        "show football items"
    ],

    # =========================
    # 📚 RAG (KNOWLEDGE)
    # =========================
    "rag": [
        "what is ideal price",
        "price range for products",
        "how many images needed",
        "what makes a good listing",
        "rules for product listing",
        "guidelines for selling",
        "how to list products",
        "best practices for products"
    ]
}
intent_embeddings = None

# 🔥 MODEL
def get_intent_embeddings():
    global intent_embeddings

    if intent_embeddings is None:
        intent_embeddings = {
            key: embedder.encode(values)
            for key, values in INTENTS.items()
        }

    return intent_embeddings



def detect_intent(message):
    embeddings = get_intent_embeddings()
    query_embedding = embedder.encode([message])

    best_intent = None
    best_score = 0

    for intent, embeddings in intent_embeddings.items():
        scores = cosine_similarity(query_embedding, embeddings).flatten()
        max_score = scores.max()

        if max_score > best_score:
            best_score = max_score
            best_intent = intent

    return best_intent, best_score