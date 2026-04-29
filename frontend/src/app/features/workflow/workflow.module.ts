import { NgModule, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { ProductService } from '../../core/services/product.service';
import { WorkflowService } from '../../core/services/workflow.service';
import { ToastService } from '../../core/services/toast.service';
import { Product, WorkflowHistoryItem } from '../../core/models/api.models';

@Component({
  selector: 'app-review-dashboard',
  standalone: false,
  template: `
    <div class="page-shell">
      <section class="hero-grid">
        <div class="hero-panel">
          <div class="eyebrow">Workflow Queue</div>
          <h2>Move listings from draft to decision with a clearer review cockpit.</h2>
          <p class="muted" style="color: rgba(239, 246, 255, 0.82);">Draft products appear here first so reviewers can quickly see what is ready to enter the approval lane.</p>
          <div class="pill-row">
            <span class="soft-chip">{{ items.length }} drafts available</span>
            <span class="soft-chip">Fast submit actions</span>
          </div>
        </div>

        <div class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Next up</div>
              <h3 style="margin-top:.35rem;">Review recommendations</h3>
            </div>
          </div>
          <div class="timeline" *ngIf="items.length; else noDrafts">
            <div class="split-line" *ngFor="let item of items.slice(0, 3)">
              <div>
                <strong>{{ item.name }}</strong>
                <div class="muted">{{ item.sku }}</div>
              </div>
              <a mat-button [routerLink]="['/products', item.id]">Inspect</a>
            </div>
          </div>
        </div>
      </section>

      <section class="stats-grid">
        <div class="metric-card">
          <div class="muted">Draft queue</div>
          <div class="value">{{ items.length }}</div>
          <div class="trend">Listings waiting to enter review</div>
        </div>
        <div class="metric-card">
          <div class="muted">Submission focus</div>
          <div class="value">{{ items.length > 0 ? 'High' : 'Clear' }}</div>
          <div class="trend">A quick signal for the current queue state</div>
        </div>
      </section>

      <mat-card class="panel" style="padding: 1.25rem;">
        <div class="section-head">
          <div>
            <div class="eyebrow">Draft products</div>
            <h3 style="margin-top:.35rem;">Ready for review submission</h3>
          </div>
          <a mat-stroked-button routerLink="/workflow/approvals">Open Approvals</a>
        </div>

        <div class="grid-cards" *ngIf="items.length; else noDrafts">
          <article class="list-card" *ngFor="let p of items">
            <div class="split-line" style="padding-top:0;border-bottom:0;">
              <div>
                <h3 style="margin:0 0 .25rem;">{{ p.name }}</h3>
                <div class="muted">{{ p.sku }}</div>
              </div>
              <span class="status-pill status-draft">{{ p.status }}</span>
            </div>
            <div class="muted">Owned by {{ p.createdBy || 'Unknown owner' }}</div>
            <div class="card-actions">
              <a mat-button [routerLink]="['/products', p.id]">View</a>
              <button mat-flat-button color="primary" type="button" (click)="submit(p.id)">Submit to Review</button>
            </div>
          </article>
        </div>
      </mat-card>
    </div>

    <ng-template #noDrafts>
      <div class="empty-state">
        <strong>No draft products are waiting right now.</strong>
        <div style="margin-top:.4rem;">That usually means the creation queue is caught up.</div>
      </div>
    </ng-template>
  `
})
export class ReviewDashboardComponent {
  private readonly products = inject(ProductService);
  private readonly workflow = inject(WorkflowService);
  private readonly toast = inject(ToastService);

  items: Product[] = [];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.products.list({ status: 'Draft', page: 1, size: 12, sort: 'name' }).subscribe((value) => this.items = value);
  }

  submit(id: string): void {
    this.workflow.submit(id).subscribe({
      next: () => {
        this.toast.success('Submitted');
        this.load();
      },
      error: () => this.toast.error('Submit failed')
    });
  }
}

@Component({
  selector: 'app-approval-page',
  standalone: false,
  template: `
    <div class="page-shell">
      <div class="page-title">
        <div>
          <div class="eyebrow">Approval Lane</div>
          <h1>Approvals</h1>
          <div class="muted">Approve, reject, or publish items that are actively in review.</div>
        </div>
      </div>

      <section class="stats-grid">
        <div class="metric-card">
          <div class="muted">Items in review</div>
          <div class="value">{{ items.length }}</div>
          <div class="trend">Current approval workload</div>
        </div>
        <div class="metric-card">
          <div class="muted">Decision state</div>
          <div class="value">{{ items.length ? 'Active' : 'Idle' }}</div>
          <div class="trend">Whether reviewers have work right now</div>
        </div>
      </section>

      <mat-card class="panel" style="padding: 1.25rem;">
        <div class="section-head">
          <div>
            <div class="eyebrow">Decision deck</div>
            <h3 style="margin-top:.35rem;">Actionable products</h3>
          </div>
        </div>
        <div class="grid-cards" *ngIf="items.length; else noApprovals">
          <article class="list-card" *ngFor="let p of items">
            <div class="split-line" style="padding-top:0;border-bottom:0;">
              <div>
                <h3 style="margin:0 0 .25rem;">{{ p.name }}</h3>
                <div class="muted">{{ p.sku }}</div>
              </div>
              <span class="status-pill status-inreview">{{ p.status }}</span>
            </div>
            <div class="card-actions">
              <button mat-stroked-button color="primary" type="button" (click)="approve(p.id)">Approve</button>
              <button mat-stroked-button color="warn" type="button" (click)="reject(p.id)">Reject</button>
              <button mat-flat-button color="accent" type="button" (click)="publish(p.id)">Publish</button>
            </div>
          </article>
        </div>
      </mat-card>
    </div>

    <ng-template #noApprovals>
      <div class="empty-state">
        <strong>No products are currently waiting for approval.</strong>
        <div style="margin-top:.4rem;">The lane is clear for now.</div>
      </div>
    </ng-template>
  `
})
export class ApprovalPageComponent {
  private readonly products = inject(ProductService);
  private readonly workflow = inject(WorkflowService);
  private readonly toast = inject(ToastService);

  items: Product[] = [];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.products.list({ status: 'InReview', page: 1, size: 20, sort: 'name' }).subscribe((value) => this.items = value);
  }

  approve(id: string): void {
    this.workflow.approve(id).subscribe({
      next: () => {
        this.toast.success('Approved');
        this.load();
      },
      error: () => this.toast.error('Approve failed')
    });
  }

  reject(id: string): void {
    this.workflow.reject(id).subscribe({
      next: () => {
        this.toast.success('Rejected');
        this.load();
      },
      error: () => this.toast.error('Reject failed')
    });
  }

  publish(id: string): void {
    this.workflow.publish(id).subscribe({
      next: () => {
        this.toast.success('Published');
        this.load();
      },
      error: () => this.toast.error('Publish failed')
    });
  }
}

@Component({
  selector: 'app-workflow-history',
  standalone: false,
  template: `
    <div class="page-shell">
      <div class="page-title">
        <div>
          <div class="eyebrow">Workflow History</div>
          <h1>Activity timeline</h1>
          <div class="muted">A cleaner trace of every action taken on this product.</div>
        </div>
      </div>

      <mat-card class="panel" style="padding: 1.25rem;">
        <div class="timeline" *ngIf="history.length; else noHistory">
          <div class="timeline-item" *ngFor="let item of history">
            <div class="dot"></div>
            <div class="info-card">
              <strong>{{ item.status }}</strong>
              <div class="muted" style="margin-top:.3rem;">{{ item.actionBy }} • {{ item.timestamp | date:'medium' }}</div>
            </div>
          </div>
        </div>
      </mat-card>
    </div>

    <ng-template #noHistory>
      <div class="empty-state">
        <strong>No workflow history is available for this item.</strong>
      </div>
    </ng-template>
  `
})
export class WorkflowHistoryComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly workflow = inject(WorkflowService);

  history: WorkflowHistoryItem[] = [];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.workflow.history(id).subscribe((value) => this.history = value);
  }
}

@NgModule({
  declarations: [ReviewDashboardComponent, ApprovalPageComponent, WorkflowHistoryComponent],
  imports: [CommonModule, MatButtonModule, MatCardModule, RouterModule.forChild([
    { path: '', component: ReviewDashboardComponent },
    { path: 'approvals', component: ApprovalPageComponent },
    { path: 'history/:id', component: WorkflowHistoryComponent }
  ])]
})
export class WorkflowModule {}
