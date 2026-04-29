import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DashboardMetrics, WorkflowHistoryItem } from '../models/api.models';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private readonly api: ApiService) {}

  dashboard(): Observable<DashboardMetrics> {
    return this.api.get<DashboardMetrics>('/gateway/reports/dashboard');
  }

  audit(id: string): Observable<WorkflowHistoryItem[]> {
    return this.api.get<WorkflowHistoryItem[]>(`/gateway/audit/products/${id}`);
  }
}
