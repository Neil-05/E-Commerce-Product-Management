import numpy as np
from price_rules import check_price_validity

def predict_product(data, model, scaler):
    description = data.description
    image_count = data.imageCount
    price = data.price
    category = data.category

    issues = []

    # 🔥 Price validation
    valid_price, reason = check_price_validity(description, category, price)

    if not valid_price:
        issues.append(reason)

    # 🔥 Other quality checks
    if len(description) < 50:
        issues.append("Description too short (min 50 characters recommended)")

    if image_count < 2:
        issues.append("Not enough images (at least 2 required)")

    # 🔮 ML Prediction
    features = np.array([[len(description), image_count, price]])
    features_scaled = scaler.transform(features)

    prediction = model.predict(features_scaled)[0]
    confidence = model.predict_proba(features_scaled)[0][1]

    # 🔥 Override if critical issues
    if issues:
        prediction = 0
        confidence = max(confidence, 0.8)  # ensure strong rejection confidence

    # 📦 Response
    response = {
        "prediction": int(prediction),
        "confidence": float(confidence)
    }

    # ✅ Add explainability
    if issues:
        response["issues"] = issues
    else:
        response["message"] = "Product looks good and likely to be approved"

    return response