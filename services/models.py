from pydantic import BaseModel, Field

class ProductRequest(BaseModel):
    description: str = Field(..., example="High quality smartphone with great camera")
    imageCount: int = Field(..., example=3)
    price: float = Field(..., example=50000)
    category: str = Field(..., example="Electronics")


class RecommendRequest(BaseModel):
    description: str = Field(..., example="smartphone with good battery")