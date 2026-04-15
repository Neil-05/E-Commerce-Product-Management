import pandas as pd
import numpy as np
from sklearn.preprocessing import LabelEncoder
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score
import joblib
import warnings
import os

warnings.filterwarnings('ignore')

MODEL_PATH = "model.pkl"

def train_and_save_model(df: pd.DataFrame):
    """
    Train the model and save it to model.pkl.
    """
    if df.empty:
        print("Dataset is empty. Cannot train model.")
        return

    # Part 2: DATA PREPROCESSING
    # Note: User specified 'ImageCount' in queries but python code generally converts to lower or uses directly.
    # We will use exactly what comes from query: description_length, ImageCount, Price, CategoryName, label
    
    le = LabelEncoder()
    # Assuming CategoryName might have unknown categories, fit_transform works fine on all data
    df['Category'] = le.fit_transform(df['CategoryName'])
    
    features = ['description_length', 'ImageCount', 'Price', 'Category']
    X = df[features].values # use numpy arrays
    y = df['label'].values
    
    # Part 3: PRODUCT APPROVAL AI
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
    
    model = LogisticRegression(max_iter=1000)
    model.fit(X_train, y_train)
    
    y_pred = model.predict(X_test)
    accuracy = accuracy_score(y_test, y_pred)
    print(f"Model trained successfully. Accuracy: {accuracy * 100:.2f}%")
    
    # Save the model
    model_data = {
        'model': model,
        'label_encoder': le
    }
    joblib.dump(model_data, MODEL_PATH)
    print(f"Model and LabelEncoder saved to {MODEL_PATH}")
    return accuracy

def load_model():
    """
    Load the model and label encoder from model.pkl.
    Returns (model, label_encoder) or (None, None) if not found.
    """
    if not os.path.exists(MODEL_PATH):
        print(f"Model file {MODEL_PATH} not found.")
        return None, None
        
    try:
        model_data = joblib.load(MODEL_PATH)
        return model_data['model'], model_data['label_encoder']
    except Exception as e:
        print(f"Failed to load model: {e}")
        return None, None

def predict_approval(model, le, description_length: int, image_count: int, price: float, category: str):
    """
    Make a prediction for a single product.
    """
    if model is None or le is None:
        raise ValueError("Model is not loaded.")
        
    try:
        # Handle unknown categories gracefully
        if category in le.classes_:
            encoded_category = le.transform([category])[0]
        else:
            # use a default/unknown if we haven't seen it, or roughly -1
            encoded_category = -1
            
        features = np.array([[description_length, image_count, price, encoded_category]])
        
        prediction = model.predict(features)[0]
        # LogisticRegression has predict_proba
        probabilities = model.predict_proba(features)[0]
        confidence = probabilities[prediction]
        
        return int(prediction), float(confidence)
    except Exception as e:
        print(f"Error during prediction: {e}")
        raise
