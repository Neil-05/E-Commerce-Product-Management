import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Product } from '../models/api.models';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class ProductService {
  constructor(private readonly api: ApiService) {}

  list(params: Record<string, string | number | boolean | undefined> = {}): Observable<Product[]> {
    return this.api.get<Product[]>('/gateway/products/plp', params);
  }

  get(id: string): Observable<Product> {
    return this.api.get<Product>(`/gateway/products/${id}`);
  }

  create(body: { name: string; sku: string; categoryId: string; description: string }): Observable<string> {
    return this.api.post<string>('/gateway/products', body);
  }

  update(id: string, body: { name: string; description: string }): Observable<string> {
    return this.api.put<string>(`/gateway/products/${id}`, body);
  }

  delete(id: string): Observable<string> {
    return this.api.delete<string>(`/gateway/products/${id}`);
  }

  upload(id: string, file: File, isPrimary = true): Observable<string> {
    const form = new FormData();
    form.append('file', file);
    form.append('isPrimary', String(isPrimary));
    return this.api.post<string>(`/gateway/products/${id}/upload`, form);
  }
}
