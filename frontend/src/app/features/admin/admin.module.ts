import { NgModule, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AdminService } from '../../core/services/admin.service';
import { DashboardMetrics, WorkflowHistoryItem } from '../../core/models/api.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: false,
  template: `
    <div class="page-shell" *ngIf="metrics">
      <section class="hero-grid">
        <div class="hero-panel">
          <div class="eyebrow">Admin Insights</div>
          <h2>Monitor catalog throughput and publishing health from one command view.</h2>
          <p class="muted" style="color: rgba(239, 246, 255, 0.82);">These KPIs turn the raw dashboard payload into something easier to scan and act on.</p>
        </div>

        <div class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Performance</div>
              <h3 style="margin-top:.35rem;">Publishing conversion</h3>
            </div>
            <strong>{{ publishRate }}%</strong>
          </div>
          <div class="split-line">
            <div>
              <strong>Approved but not yet published</strong>
              <div class="muted">Potential bottleneck between approval and launch.</div>
            </div>
            <strong>{{ approvalGap }}</strong>
          </div>
          <div class="card-actions">
            <a mat-stroked-button routerLink="/admin/audit">Open Audit Logs</a>
          </div>
        </div>
      </section>

      <section class="stats-grid">
        <div class="metric-card">
          <div class="muted">Total products</div>
          <div class="value">{{ metrics.totalProducts }}</div>
          <div class="trend">Overall catalog volume</div>
        </div>
        <div class="metric-card">
          <div class="muted">Approved</div>
          <div class="value">{{ metrics.approvedProducts }}</div>
          <div class="trend">Listings that passed review</div>
        </div>
        <div class="metric-card">
          <div class="muted">Published</div>
          <div class="value">{{ metrics.publishedProducts }}</div>
          <div class="trend">Listings live in the market</div>
        </div>
      </section>
    </div>
  `
})
export class DashboardComponent {
  private readonly service = inject(AdminService);
  metrics?: DashboardMetrics;

  get publishRate(): number {
    if (!this.metrics?.approvedProducts) return 0;
    return Math.round((this.metrics.publishedProducts / this.metrics.approvedProducts) * 100);
  }

  get approvalGap(): number {
    if (!this.metrics) return 0;
    return Math.max(0, this.metrics.approvedProducts - this.metrics.publishedProducts);
  }

  ngOnInit(): void {
    this.service.dashboard().subscribe((value) => this.metrics = value);
  }
}

@Component({
  selector: 'app-audit-logs',
  standalone: false,
  template: `
    <div class="page-shell">
      <div class="page-title">
        <div>
          <div class="eyebrow">Audit Explorer</div>
          <h1>Audit logs</h1>
          <div class="muted">Look up a product by ID and review every recorded workflow event.</div>
        </div>
      </div>

      <mat-card class="panel" style="padding: 1.25rem;">
        <form [formGroup]="form" class="form-grid">
          <mat-form-field>
            <mat-label>Product ID</mat-label>
            <input matInput formControlName="productId" placeholder="Enter a product UUID">
          </mat-form-field>
          <div class="card-actions">
            <button mat-flat-button color="primary" type="button" (click)="load()">Load Audit Trail</button>
          </div>
        </form>
      </mat-card>

      <mat-card class="panel" style="padding: 1.25rem;">
        <div class="section-head">
          <div>
            <div class="eyebrow">Results</div>
            <h3 style="margin-top:.35rem;">Activity records</h3>
          </div>
          <div class="muted">{{ items.length }} events</div>
        </div>

        <div class="timeline" *ngIf="items.length; else noAudit">
          <div class="timeline-item" *ngFor="let item of items">
            <div class="dot"></div>
            <div class="info-card">
              <strong>{{ item.status }}</strong>
              <div class="muted" style="margin-top:.3rem;">{{ item.actionBy }} • {{ item.timestamp | date:'medium' }}</div>
            </div>
          </div>
        </div>
      </mat-card>
    </div>

    <ng-template #noAudit>
      <div class="empty-state">
        <strong>No audit records loaded yet.</strong>
        <div style="margin-top:.4rem;">Enter a product ID to fetch its workflow trace.</div>
      </div>
    </ng-template>
  `
})
export class AuditLogsComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AdminService);

  readonly form = this.fb.group({ productId: ['', Validators.required] });
  items: WorkflowHistoryItem[] = [];

  load(): void {
    if (this.form.invalid) return;
    this.service.audit(this.form.getRawValue().productId!).subscribe((value) => this.items = value);
  }
}

@NgModule({
  declarations: [DashboardComponent, AuditLogsComponent],
  imports: [CommonModule, ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, RouterModule.forChild([
    { path: '', component: DashboardComponent },
    { path: 'audit', component: AuditLogsComponent }
  ])]
})
export class AdminModule {}
