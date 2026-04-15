import os
import pyodbc
import pandas as pd
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Configurable Connection String (could use pydantic BaseSettings in a larger app, but simple env works here)
# Default assumes a local SQL server or connection provided via DB_CONNECTION_STRING
DB_CONNECTION_STRING = os.getenv(
    "DB_CONNECTION_STRING",
    "Driver={ODBC Driver 17 for SQL Server};Server=localhost;Database=eCommerceDB;Trusted_Connection=yes;"
)

def get_db_connection():
    """Establish and return a database connection."""
    try:
        conn = pyodbc.connect(DB_CONNECTION_STRING)
        return conn
    except Exception as e:
        logger.error(f"Failed to connect to database: {e}")
        return None

def fetch_training_data() -> pd.DataFrame:
    """Fetch product data for model training from the SQL Server."""
    query = """
    SELECT
        LEN(Description) AS description_length,
        ImageCount,
        Price,
        CategoryName,
        CASE
            WHEN Status = 'Approved' THEN 1
            ELSE 0
        END AS label
    FROM Products
    """
    conn = get_db_connection()
    if conn is None:
        # If DB connection fails, return an empty dataframe with correct columns
        # so that the process fails gracefully or relies on mock data
        logger.warning("Returning empty DataFrame due to database connection failure.")
        return pd.DataFrame(columns=["description_length", "ImageCount", "Price", "CategoryName", "label"])

    try:
        df = pd.read_sql_query(query, conn)
        # Handle missing/null values
        df['description_length'] = df['description_length'].fillna(0)
        df['ImageCount'] = df['ImageCount'].fillna(0)
        df['Price'] = df['Price'].fillna(0.0)
        df['CategoryName'] = df['CategoryName'].fillna("Unknown")
        
        return df
    except Exception as e:
        logger.error(f"Error executing query: {e}")
        return pd.DataFrame(columns=["description_length", "ImageCount", "Price", "CategoryName", "label"])
    finally:
        conn.close()

def fetch_all_products_for_recommendation() -> pd.DataFrame:
    """Fetch all products for the recommendation system (returns base descriptions)."""
    query = """
    SELECT
        Id as id,
        Name as name,
        Description as description,
        CategoryName as category
    FROM Products
    """
    conn = get_db_connection()
    if conn is None:
        return pd.DataFrame(columns=["id", "name", "description", "category"])
    
    try:
        df = pd.read_sql_query(query, conn)
        df['description'] = df['description'].fillna("")
        df['name'] = df['name'].fillna("Unknown Product")
        df['category'] = df['category'].fillna("Unknown")
        return df
    except Exception as e:
        logger.error(f"Error fetching products for recommendation: {e}")
        return pd.DataFrame(columns=["id", "name", "description", "category"])
    finally:
        conn.close()
