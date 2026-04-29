import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { WorkflowHistoryItem } from '../models/api.models';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class WorkflowService {
  constructor(private readonly api: ApiService) {}

  submit(id: string): Observable<string> { return this.api.post<string>(`/gateway/workflow/submit/${id}`, {}); }
  approve(id: string): Observable<string> { return this.api.post<string>(`/gateway/workflow/approve/${id}`, {}); }
  reject(id: string): Observable<string> { return this.api.post<string>(`/gateway/workflow/reject/${id}`, {}); }
  publish(id: string): Observable<string> { return this.api.post<string>(`/gateway/workflow/publish/${id}`, {}); }
  history(id: string): Observable<WorkflowHistoryItem[]> { return this.api.get<WorkflowHistoryItem[]>(`/gateway/workflow/history/${id}`); }
  updatePricing(id: string, body: { price: number; discount: number }): Observable<string> { return this.api.put<string>(`/gateway/workflow/products/${id}/pricing`, body); }
  updateInventory(id: string, body: { stock: number }): Observable<string> { return this.api.put<string>(`/gateway/workflow/products/${id}/inventory`, body); }
}
