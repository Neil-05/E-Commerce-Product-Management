from fastapi import FastAPI, HTTPException, status
from pydantic import BaseModel, Field
import uvicorn
import logging

import db
import model
from utils.scoring import calculate_score
from utils.feedback import generate_feedback
from utils.recommendation import get_recommendations

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Global variables for model and LabelEncoder
GLOBAL_MODEL = None
GLOBAL_LE = None

# Application setup
app = FastAPI(
    title="eCommerce Product Validation System",
    description="AI/ML Microservice for validating and scoring eCommerce products.",
    version="1.0.0"
)

# Load model at startup
@app.on_event("startup")
def startup_event():
    global GLOBAL_MODEL, GLOBAL_LE
    logger.info("Starting up and loading machine learning model...")
    GLOBAL_MODEL, GLOBAL_LE = model.load_model()
    if GLOBAL_MODEL is None:
        logger.warning("No pre-trained model found. Please hit the /train endpoint.")
    else:
        logger.info("Model loaded successfully.")

# Pydantic models for incoming requests
class ProductData(BaseModel):
    description: str = Field(..., description="Full text description of the product")
    imageCount: int = Field(..., description="Number of images provided")
    price: float = Field(..., description="Price of the product")
    category: str = Field(..., description="Category name")

class PredictionRequest(ProductData):
    pass # Reusing ProductData schema

class RecommendRequest(BaseModel):
    description: str = Field(..., description="Description of the product to find recommendations for")

@app.get("/")
def root():
    """Health check endpoint."""
    return {"message": "ML Service Running"}

@app.post("/train")
def train_model():
    """Pull data from the database and retrain the machine learning model."""
    global GLOBAL_MODEL, GLOBAL_LE
    df = db.fetch_training_data()
    if df.empty:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to fetch training data from the database or dataset is empty."
        )
    
    accuracy = model.train_and_save_model(df)
    
    # Reload model into memory
    GLOBAL_MODEL, GLOBAL_LE = model.load_model()
    
    return {
        "message": "Model trained and saved successfully.",
        "accuracy": accuracy
    }

@app.post("/predict")
def predict_product(data: PredictionRequest):
    """Predict whether a product will be approved or rejected based on its attributes."""
    if GLOBAL_MODEL is None or GLOBAL_LE is None:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="Model is not loaded. Please train the model first."
        )
    
    description_length = len(data.description)
    try:
        prediction, confidence = model.predict_approval(
            model=GLOBAL_MODEL,
            le=GLOBAL_LE,
            description_length=description_length,
            image_count=data.imageCount,
            price=data.price,
            category=data.category
        )
        return {
            "prediction": prediction,
            "confidence": confidence
        }
    except Exception as e:
        logger.error(f"Prediction error: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Prediction failed: {str(e)}"
        )

@app.post("/score")
def score_product(data: ProductData):
    """Calculate completeness score for a product."""
    data_dict = data.dict()
    return calculate_score(data_dict)

@app.post("/feedback")
def get_feedback(data: ProductData):
    """Generate artificial feedback for a product configuration."""
    data_dict = data.dict()
    return generate_feedback(data_dict)

@app.post("/recommend")
def recommend_products(req: RecommendRequest):
    """Return top 5 similar products using TF-IDF."""
    try:
        return get_recommendations(req.description)
    except Exception as e:
        logger.error(f"Recommendation error: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to generate recommendations."
        )

@app.post("/validate")
def validate_product(data: ProductData):
    """Combine ML Prediction, Scoring, and Feedback into one comprehensive validation decision."""
    
    # Get score
    score_result = calculate_score(data.dict())
    score = score_result["score"]
    
    # Get feedback
    feedback_result = generate_feedback(data.dict())
    
    # Try prediction (fall back if no model is loaded)
    prediction = 0
    confidence = 0.0
    if GLOBAL_MODEL is not None and GLOBAL_LE is not None:
        try:
            description_length = len(data.description)
            prediction, confidence = model.predict_approval(
                model=GLOBAL_MODEL,
                le=GLOBAL_LE,
                description_length=description_length,
                image_count=data.imageCount,
                price=data.price,
                category=data.category
            )
        except Exception as e:
            logger.error(f"Validation prediction error: {e}. Falling back to default prediction 0.")
            prediction = 0
            feedback_result["issues"].append("Model unavailable, default rejection applied.")
    else:
        logger.warning("No model available for validation prediction.")
        feedback_result["issues"].append("Prediction model not active.")
        
    # Decision logic
    if prediction == 0 or score < 60:
        decision = "Rejected"
    else:
        decision = "Approved"
        
    return {
        "decision": decision,
        "confidence": float(confidence),
        "score": score,
        "issues": feedback_result["issues"],
        "suggestion": feedback_result["suggestion"]
    }

if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
