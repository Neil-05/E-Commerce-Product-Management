import pandas as pd
from sklearn.metrics.pairwise import cosine_similarity
from model_loader import load_models

products_df = pd.read_csv("data/products.csv")

_,_,embedder=load_models()

product_embeddings = None  # 🔥 initially empty


def get_embeddings():
    global product_embeddings

    # 🔥 compute only once AFTER embedder is ready
    if product_embeddings is None:
        product_embeddings = embedder.encode(
            products_df["description"].tolist()
        )

    return product_embeddings


def recommend_products(description):
    if not description:
        return {"error": "Description required"}

    embeddings = get_embeddings()  # 🔥 safe call

    query_embedding = embedder.encode([description])

    similarities = cosine_similarity(query_embedding, embeddings).flatten()

    top_indices = similarities.argsort()[-5:][::-1]

    results = []
    for idx in top_indices:
        results.append({
            "name": products_df.iloc[idx]["name"],
            "category": products_df.iloc[idx]["category"],
            "similarity": float(similarities[idx])
        })

    return results