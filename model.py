import pandas as pd
from sklearn.linear_model import LogisticRegression
from sklearn.preprocessing import StandardScaler
import pickle


# Load data
data = pd.read_csv("data/training_data.csv")

# Features
X = data[["description_length", "image_count", "price"]]
y = data["label"]

# 🔥 ADD SCALER
scaler = StandardScaler()
X_scaled = scaler.fit_transform(X)

model = LogisticRegression()
model.fit(X_scaled, y)

# ✅ SAVE BOTH
with open("model.pkl", "wb") as f:
    pickle.dump(model, f)

with open("scaler.pkl", "wb") as f:
    pickle.dump(scaler, f)

print("Model + Scaler saved")