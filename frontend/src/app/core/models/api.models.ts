export interface AuthResponse {
  token: string;
  refreshToken?: string | null;
  email: string;
  role: string;
}

export interface UserProfile {
  email: string;
  role: string;
  name?: string;
}

export interface Product {
  id: string;
  name: string;
  sku: string;
  status: string;
  createdBy: string;
  description?: string;
  categoryId?: string;
  price?: number;
  discount?: number;
  stock?: number;
  images?: string[];
}

export interface WorkflowHistoryItem {
  status: string;
  actionBy: string;
  timestamp: string;
}

export interface DashboardMetrics {
  totalProducts: number;
  approvedProducts: number;
  publishedProducts: number;
}
