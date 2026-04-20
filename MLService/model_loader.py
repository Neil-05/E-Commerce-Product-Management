from sentence_transformers import SentenceTransformer
import pickle

model = None
scaler = None
embedder = None

def load_models():
    global model, scaler, embedder

    if model is None:
        with open("model.pkl", "rb") as f:
            model = pickle.load(f)

    if scaler is None:
        with open("scaler.pkl", "rb") as f:
            scaler = pickle.load(f)

    if embedder is None:
        embedder = SentenceTransformer('all-MiniLM-L6-v2')

    return model, scaler, embedder