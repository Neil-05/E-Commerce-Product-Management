import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
import sys
import os

# Add parent directory to path to allow importing db
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
import db

def get_recommendations(description: str, products_df: pd.DataFrame = None) -> list:
    """
    Get top 5 similar products based on description using TF-IDF and cosine similarity.
    """
    if not description:
        return []

    # Fetch products from the database if not provided
    if products_df is None:
        products_df = db.fetch_all_products_for_recommendation()
    
    if products_df.empty:
        return []

    # Combine the new description with the existing descriptions
    all_descriptions = products_df['description'].tolist()
    all_descriptions.append(description)

    # Compute TF-IDF
    vectorizer = TfidfVectorizer(stop_words='english')
    try:
        tfidf_matrix = vectorizer.fit_transform(all_descriptions)
    except ValueError:
        # Happens if vocab is empty (e.g. all stop words)
        return []

    # Compute cosine similarity between the new description (last row) and all product descriptions (excluding last row)
    target_vector = tfidf_matrix[-1]
    product_vectors = tfidf_matrix[:-1]
    
    similarities = cosine_similarity(target_vector, product_vectors).flatten()

    # Add similarities to the dataframe to easily sort
    products_df['similarity'] = similarities

    # Sort by similarity descending
    top_5 = products_df.sort_values(by='similarity', ascending=False).head(5)

    # Filter out ones with 0 similarity if desired, but we will return top 5 regardless
    recommendations = []
    for _, row in top_5.iterrows():
        # if row['similarity'] > 0: # Option to return only if there is some matching
        recommendations.append({
            "name": row['name'],
            "category": row['category'],
            "similarity": float(row['similarity'])
        })

    return recommendations
