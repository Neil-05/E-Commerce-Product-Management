from fastapi import FastAPI
from model_loader import load_models
from services.predict_service import predict_product
from services.score_service import calculate_score
from services.feedback_service import generate_feedback
from services.validate_service import validate_product
from services.recommend_service import recommend_products
from services.models import ProductRequest, RecommendRequest
from pydantic import BaseModel
from services.validate_service import validate_product
from services.recommend_service import recommend_products
from services.rag_service import retrieve_context
from intent_detector import detect_intent
from parser import parse_product_from_text

class ChatRequest(BaseModel):
    message: str

app = FastAPI()

model, scaler, _= load_models()


@app.on_event("startup")
def startup_event():
    global model, scaler, embedder
    model, scaler, embedder = load_models()

@app.get("/")
def home():
    return {"message": "ML Service Running"}

@app.post("/predict")
def predict(data: ProductRequest):
    return predict_product(data, model, scaler)

@app.post("/score")
def score(data: ProductRequest):
    return calculate_score(data)

@app.post("/feedback")
def feedback(data: ProductRequest):
    return generate_feedback(data)

@app.post("/validate")
def validate(data: ProductRequest):
    return validate_product(data, model, scaler)

@app.post("/recommend")
def recommend(data: RecommendRequest):
    return recommend_products(data.description)


@app.post("/chat")
def chat(data: ChatRequest):
    message = data.message.lower()

    # =========================
    # 🔥 STEP 1: RULE-BASED INTENT (PRIORITY)
    # =========================
    if any(word in message for word in ["feedback", "wrong", "issue", "problem", "fix", "improve"]):
        intent = "feedback"

    elif any(word in message for word in ["score", "rate"]):
        intent = "score"

    elif any(word in message for word in ["recommend", "suggest"]):
        intent = "recommend"

    elif any(word in message for word in ["validate", "check", "good"]):
        intent = "validate"

    elif any(word in message for word in ["predict", "approval"]):
        intent = "predict"

    # =========================
    # 🤖 STEP 2: ML INTENT (FALLBACK)
    # =========================
    else:
        intent, score = detect_intent(message)

        # =========================
        # 📚 STEP 3: RAG FALLBACK
        # =========================
        if score < 0.3:
            intent = "rag"

    # =========================
    # 🔮 PREDICT
    # =========================
    if intent == "predict":
        parsed = parse_product_from_text(message)
        result = predict_product(parsed, model, scaler)

        return {
            "reply": "Here is the approval prediction:",
            "data": result
        }

    # =========================
    # 🎯 VALIDATE
    # =========================
    elif intent == "validate":
        parsed = parse_product_from_text(message)
        result = validate_product(parsed, model, scaler)

        return {
            "reply": "Here is the validation result:",
            "data": result
        }

    # =========================
    # 📊 SCORE
    # =========================
    elif intent == "score":
        parsed = parse_product_from_text(message)
        result = calculate_score(parsed)

        return {
            "reply": "Here is the product score:",
            "data": result
        }

    # =========================
    # 💬 FEEDBACK
    # =========================
    elif intent == "feedback":
        parsed = parse_product_from_text(message)
        result = generate_feedback(parsed)

        return {
            "reply": "Here are the issues:",
            "data": result
        }

    # =========================
    # 🤖 RECOMMEND
    # =========================
    elif intent == "recommend":
        results = recommend_products(message)

        return {
            "reply": "Here are some recommended products:",
            "data": results
        }

    # =========================
    # 📚 RAG (DEFAULT)
    # =========================
    else:
        context = retrieve_context(message)

        return {
            "reply": f"Based on our guidelines:\n{context}"
        }