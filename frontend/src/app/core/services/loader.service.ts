import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoaderService {
  private readonly count = new BehaviorSubject(0);
  readonly loading$ = this.count.asObservable();
  start(): void { this.count.next(this.count.value + 1); }
  stop(): void { this.count.next(Math.max(0, this.count.value - 1)); }
}
