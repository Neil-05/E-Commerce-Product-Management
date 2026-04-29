import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class AiService {
  constructor(private readonly api: ApiService) {}

  validate(body: unknown): Observable<unknown> { return this.api.mlPost('/validate', body); }
  predict(body: unknown): Observable<unknown> { return this.api.mlPost('/predict', body); }
  feedback(body: unknown): Observable<unknown> { return this.api.mlPost('/feedback', body); }
  chat(body: unknown): Observable<unknown> { return this.api.mlPost('/chat', body); }
}
