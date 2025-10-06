import { CreateOrderDto, CreateProductDto, Order, Product, UpdateProductDto } from '@/types';
import { generateIdempotencyKey } from '@/util/idempotency';
import axios, { AxiosError } from 'axios';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

// Interface para errores de la API
interface ApiError {
  error?: string;
  message?: string;
}

class ApiService {
  private baseUrl: string;
  private token: string | null = null;

  constructor() {
    this.baseUrl = API_BASE_URL;
  }
  

  private async ensureToken(): Promise<string> {
    if (!this.token) {
      // En una app real, esto vendría de un contexto de autenticación
      // Por ahora, generamos un token temporal para cada petición
      try {
        const response = await axios.get(`${this.baseUrl}/api/products`);
        const authHeader = response.headers['authorization'];
        if (authHeader) {
          this.token = authHeader.replace('Bearer ', '');
        }
      } catch (error) {
        console.error('Error generating token:', error);
        this.token = 'temp-token'; // Fallback para desarrollo
      }
    }
    return this.token!;
  }

  private async getHeaders(): Promise<Record<string, string>> {
    const token = await this.ensureToken();
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    };
  }

  private async getHeadersIdenpotency(idempotencyKey: string): Promise<Record<string, string>> {
    const token = await this.ensureToken();
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
      'Idempotency-Key': `${idempotencyKey}`,
    };
  }

  private handleError(error: unknown): never {
    if (error instanceof AxiosError) {
      const errorData = error.response?.data as ApiError;
      throw new Error(errorData?.error || errorData?.message || error.message || 'An error occurred');
    }
    
    if (error instanceof Error) {
      throw error;
    }
    
    throw new Error('An unexpected error occurred');
  }

  // Products
  async getProducts(): Promise<Product[]> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.get(`${this.baseUrl}/api/products`, { headers });
      return response.data;
    } catch (error) {
      this.handleError(error);
    }
  }

  async createProduct(productData: CreateProductDto): Promise<Product> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.post(`${this.baseUrl}/api/products`, productData, { headers });
      return response.data;
    } catch (error) {
      this.handleError(error);
    }
  }

  async updateProduct(id: number, productData: UpdateProductDto): Promise<Product> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.put(`${this.baseUrl}/api/products/${id}`, productData, { headers });
      return response.data;
    } catch (error) {
      this.handleError(error);
    }
  }

  async deleteProduct(id: number): Promise<void> {
    try {
      const headers = await this.getHeaders();
      await axios.delete(`${this.baseUrl}/api/products/${id}`, { headers });
    } catch (error) {
      this.handleError(error);
    }
  }

  async decreaseStock(id: number, quantity: number): Promise<void> {
    try {
      const headers = await this.getHeaders();
      await axios.post(`${this.baseUrl}/api/products/${id}/increase-stock?qty=${quantity}`, { headers });
    } catch (error) {
      this.handleError(error);
    }
  }

  async increaseStock(id: number, quantity: number): Promise<void> {
    try {
      const headers = await this.getHeaders();
      await axios.post(`${this.baseUrl}/api/products/${id}/increase-stock?qty=${quantity}`, { headers });
    } catch (error) {
      this.handleError(error);
    }
  }

  // Orders
  async getOrders(): Promise<Order[]> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.get(`${this.baseUrl}/api/orders`, { headers });
      return response.data;
    } catch (error) {
      this.handleError(error);
    }
  }

  async createOrder(orderData: CreateOrderDto): Promise<Order> {
    try {
      const idempotencyKey = await generateIdempotencyKey(orderData);
      const headers = await this.getHeadersIdenpotency(idempotencyKey);
      const response = await axios.post(`${this.baseUrl}/api/orders`, orderData, { headers });
      return response.data;
    } catch (error) {
      this.handleError(error);
    }
  }
  
}

export const apiService = new ApiService();