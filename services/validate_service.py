from services.score_service import calculate_score
from services.feedback_service import generate_feedback
import numpy as np

def validate_product(data, model, scaler):
    description = data.description
    image_count = data.imageCount
    price = data.price
    

    features = np.array([[len(description), image_count, price]])
    features_scaled = scaler.transform(features)

    prediction = model.predict(features_scaled)[0]
    confidence = model.predict_proba(features_scaled)[0][1]

    score_data = calculate_score(data)
    feedback_data = generate_feedback(data)

    decision = "Approved"
    if prediction == 0 or score_data["score"] < 60:
        decision = "Rejected"

    return {
        "decision": decision,
        "confidence": float(confidence),
        "score": score_data["score"],
        "issues": feedback_data["issues"],
        "suggestion": feedback_data["suggestion"]
    }