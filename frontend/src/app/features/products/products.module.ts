import { NgModule, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ProductService } from '../../core/services/product.service';
import { ToastService } from '../../core/services/toast.service';
import { Product, WorkflowHistoryItem } from '../../core/models/api.models';
import { WorkflowService } from '../../core/services/workflow.service';
import { AiService } from '../../core/services/ai.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-product-list',
  standalone: false,
  template: `
    <div class="page-shell">
      <section class="hero-grid">
        <div class="hero-panel">
          <div class="eyebrow">Product Catalog</div>
          <h2>Build, review, and launch listings with a much stronger operating view.</h2>
          <p class="muted" style="color: rgba(239, 246, 255, 0.82);">Search the catalog, inspect readiness, and jump straight into the products that need attention.</p>
          <div class="pill-row">
            <span class="soft-chip">{{ products.length }} visible products</span>
            <span class="soft-chip">{{ statusCount('Draft') }} drafts</span>
            <span class="soft-chip">{{ statusCount('InReview') }} in review</span>
          </div>
        </div>

        <div class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Featured</div>
              <h3 style="margin-top:.35rem;">{{ featuredProduct?.name || 'No products yet' }}</h3>
            </div>
            <span *ngIf="featuredProduct" class="status-pill" [ngClass]="statusClass(featuredProduct.status)">{{ featuredProduct.status }}</span>
          </div>
          <p class="muted">{{ featuredProduct?.sku || 'Create your first listing to populate the workspace.' }}</p>
          <div class="value-grid" *ngIf="featuredProduct">
            <div class="value-tile">
              Owner
              <strong>{{ featuredProduct.createdBy || 'Unassigned' }}</strong>
            </div>
            <div class="value-tile">
              Images
              <strong>{{ featuredProduct.images?.length || 0 }}</strong>
            </div>
          </div>
          <div class="card-actions" *ngIf="featuredProduct">
            <a mat-flat-button color="primary" [routerLink]="['/products', featuredProduct.id]">Open Detail</a>
            <a mat-stroked-button color="primary" [routerLink]="['/products', featuredProduct.id, 'edit']">Refine Listing</a>
          </div>
        </div>
      </section>

      <section class="stats-grid">
        <div class="metric-card">
          <div class="muted">Total visible</div>
          <div class="value">{{ products.length }}</div>
          <div class="trend">Filtered workspace result set</div>
        </div>
        <div class="metric-card">
          <div class="muted">Ready to review</div>
          <div class="value">{{ statusCount('Draft') }}</div>
          <div class="trend">Draft items that can move forward</div>
        </div>
        <div class="metric-card">
          <div class="muted">Active approvals</div>
          <div class="value">{{ statusCount('InReview') }}</div>
          <div class="trend">Listings waiting on decisions</div>
        </div>
        <div class="metric-card">
          <div class="muted">Published</div>
          <div class="value">{{ statusCount('Published') }}</div>
          <div class="trend">Live catalog items in this view</div>
        </div>
      </section>

      <mat-card class="panel" style="padding: 1.25rem;">
        <div class="section-head">
          <div>
            <div class="eyebrow">Filters</div>
            <h3 style="margin-top:.35rem;">Catalog controls</h3>
          </div>
          <div class="page-actions">
            <button mat-stroked-button type="button" (click)="clearFilters()">Reset</button>
            <a mat-flat-button color="primary" routerLink="/products/new">Create Product</a>
          </div>
        </div>

        <form [formGroup]="filters" class="form-grid">
          <mat-form-field>
            <mat-label>Search</mat-label>
            <input matInput formControlName="search" placeholder="Search by name or SKU">
          </mat-form-field>
          <mat-form-field>
            <mat-label>Status</mat-label>
            <mat-select formControlName="status">
              <mat-option value="">All</mat-option>
              <mat-option *ngFor="let s of statuses" [value]="s">{{ s }}</mat-option>
            </mat-select>
          </mat-form-field>
          <div class="card-actions">
            <button mat-flat-button color="primary" type="button" (click)="load()">Apply Filters</button>
          </div>
        </form>

        <div class="chip-row" style="margin-top:1rem;">
          <button
            type="button"
            class="filter-pill"
            [class.active]="!filters.value.status"
            (click)="setStatus('')"
          >
            All
          </button>
          <button
            *ngFor="let status of statuses"
            type="button"
            class="filter-pill"
            [class.active]="filters.value.status === status"
            (click)="setStatus(status)"
          >
            {{ status }}
          </button>
        </div>
      </mat-card>

      <section class="grid-2">
        <mat-card class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Catalog Feed</div>
              <h3 style="margin-top:.35rem;">Product list</h3>
            </div>
            <div class="muted">{{ products.length }} items</div>
          </div>

          <div class="grid-cards" *ngIf="products.length; else noProducts">
            <article class="list-card" *ngFor="let p of products">
              <div class="split-line" style="padding-top:0;border-bottom:0;">
                <div>
                  <h3 style="margin:0 0 .25rem;">{{ p.name }}</h3>
                  <div class="muted">{{ p.sku }}</div>
                </div>
                <span class="status-pill" [ngClass]="statusClass(p.status)">{{ p.status }}</span>
              </div>
              <div class="muted">Created by {{ p.createdBy || 'Unknown owner' }}</div>
              <div class="pill-row" style="margin-top:.85rem;">
                <span class="soft-chip" style="background:rgba(15,98,254,.08); color:var(--primary-deep); border-color:rgba(15,98,254,.12);">{{ p.images?.length || 0 }} media</span>
              </div>
              <div class="card-actions">
                <a mat-button [routerLink]="['/products', p.id]">View</a>
                <a mat-button [routerLink]="['/products', p.id, 'edit']">Edit</a>
                <button mat-button color="warn" type="button" (click)="remove(p.id)">Delete</button>
              </div>
            </article>
          </div>
        </mat-card>

        <mat-card class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Snapshot</div>
              <h3 style="margin-top:.35rem;">Status breakdown</h3>
            </div>
          </div>
          <div class="timeline">
            <div class="split-line" *ngFor="let status of statuses">
              <div>
                <strong>{{ status }}</strong>
                <div class="muted">Items currently in this stage</div>
              </div>
              <strong>{{ statusCount(status) }}</strong>
            </div>
          </div>
        </mat-card>
      </section>
    </div>

    <ng-template #noProducts>
      <div class="empty-state">
        <strong>No products matched the current filters.</strong>
        <div style="margin-top:.4rem;">Try resetting the filters or create a new product to get started.</div>
      </div>
    </ng-template>
  `
})
export class ProductListComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ProductService);
  private readonly toast = inject(ToastService);

  readonly statuses = ['Draft', 'InReview', 'Approved', 'Rejected', 'Published'];
  readonly filters = this.fb.group({ search: [''], status: [''] });
  products: Product[] = [];

  get featuredProduct(): Product | undefined {
    return this.products[0];
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    const raw = this.filters.getRawValue();
    this.service.list({
      search: raw.search ?? undefined,
      status: raw.status ?? undefined,
      page: 1,
      size: 20,
      sort: 'name'
    }).subscribe({
      next: (value) => this.products = value,
      error: () => this.toast.error('Unable to load products')
    });
  }

  setStatus(status: string): void {
    this.filters.patchValue({ status });
    this.load();
  }

  clearFilters(): void {
    this.filters.reset({ search: '', status: '' });
    this.load();
  }

  statusCount(status: string): number {
    return this.products.filter((product) => product.status === status).length;
  }

  statusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }

  remove(id: string): void {
    this.service.delete(id).subscribe({
      next: () => {
        this.toast.success('Deleted');
        this.load();
      },
      error: () => this.toast.error('Delete failed')
    });
  }
}

@Component({
  selector: 'app-product-detail',
  standalone: false,
  template: `
    <div *ngIf="product" class="page-shell">
      <section class="hero-grid">
        <div class="hero-panel">
          <div class="eyebrow">Product Detail</div>
          <h2>{{ product.name }}</h2>
          <p class="muted" style="color: rgba(239, 246, 255, 0.82);">{{ product.sku }}{{ product.createdBy ? ' • ' + product.createdBy : '' }}</p>
          <div class="pill-row">
            <span class="soft-chip">ID {{ product.id.slice(0, 8) }}</span>
            <span class="soft-chip">{{ product.images?.length || 0 }} media assets</span>
          </div>
        </div>

        <div class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Current stage</div>
              <h3 style="margin-top:.35rem;">Workflow status</h3>
            </div>
            <span class="status-pill" [ngClass]="statusClass(product.status)">{{ product.status }}</span>
          </div>
          <div class="value-grid">
            <div class="value-tile">
              Media
              <strong>{{ product.images?.length || 0 }}</strong>
            </div>
            <div class="value-tile">
              History events
              <strong>{{ history.length }}</strong>
            </div>
          </div>
          <div class="card-actions">
            <a mat-flat-button color="primary" [routerLink]="['/products', product.id, 'edit']">Edit Product</a>
            <a mat-stroked-button color="primary" [routerLink]="['/workflow/history', product.id]">Open Timeline</a>
          </div>
        </div>
      </section>

      <section class="grid-2">
        <mat-card class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Overview</div>
              <h3 style="margin-top:.35rem;">Listing summary</h3>
            </div>
          </div>
          <div class="split-line">
            <div class="muted">Product ID</div>
            <strong>{{ product.id }}</strong>
          </div>
          <div class="split-line">
            <div class="muted">Created by</div>
            <strong>{{ product.createdBy || 'Unknown' }}</strong>
          </div>
          <div class="split-line">
            <div class="muted">Status</div>
            <strong>{{ product.status }}</strong>
          </div>

          <div style="margin-top:1rem;" *ngIf="product.images?.length; else noMedia">
            <div class="media-grid">
              <img *ngFor="let image of product.images" [src]="apiUrl + image" [alt]="product.name">
            </div>
          </div>
        </mat-card>

        <mat-card class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Workflow history</div>
              <h3 style="margin-top:.35rem;">Recent actions</h3>
            </div>
          </div>
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
      </section>
    </div>

    <ng-template #noMedia>
      <div class="empty-state">
        <strong>No media uploaded yet.</strong>
        <div style="margin-top:.4rem;">Use the edit screen to add product imagery.</div>
      </div>
    </ng-template>

    <ng-template #noHistory>
      <div class="empty-state">
        <strong>No workflow activity yet.</strong>
        <div style="margin-top:.4rem;">This listing has not moved through review.</div>
      </div>
    </ng-template>
  `
})
export class ProductDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(ProductService);
  private readonly workflow = inject(WorkflowService);

  readonly apiUrl = environment.apiUrl;
  product?: Product;
  history: WorkflowHistoryItem[] = [];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.service.get(id).subscribe((value) => this.product = value);
    this.workflow.history(id).subscribe((value) => this.history = value);
  }

  statusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }
}

@Component({
  selector: 'app-product-form',
  standalone: false,
  template: `
    <div class="page-shell">
      <div class="page-title">
        <div>
          <div class="eyebrow">Product Workspace</div>
          <h1>{{ editMode ? 'Refine product' : 'Create product' }}</h1>
          <div class="muted">A richer listing editor with live readiness checks, pricing context, and AI assistance.</div>
        </div>
      </div>

      <section class="grid-2">
        <mat-card class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Catalog editor</div>
              <h3 style="margin-top:.35rem;">Core listing details</h3>
            </div>
            <span class="status-pill status-draft">{{ editMode ? 'Editing existing item' : 'Draft in progress' }}</span>
          </div>

          <form [formGroup]="form" class="form-grid">
            <mat-form-field>
              <mat-label>Name</mat-label>
              <input matInput formControlName="name">
            </mat-form-field>
            <mat-form-field>
              <mat-label>SKU</mat-label>
              <input matInput formControlName="sku" [readonly]="editMode">
            </mat-form-field>
            <mat-form-field>
              <mat-label>Category ID</mat-label>
              <input matInput formControlName="categoryId">
            </mat-form-field>
            <mat-form-field>
              <mat-label>Price</mat-label>
              <input matInput type="number" formControlName="price">
            </mat-form-field>
            <mat-form-field>
              <mat-label>Discount</mat-label>
              <input matInput type="number" formControlName="discount">
            </mat-form-field>
            <mat-form-field>
              <mat-label>Stock</mat-label>
              <input matInput type="number" formControlName="stock">
            </mat-form-field>
            <mat-form-field class="span-2">
              <mat-label>Description</mat-label>
              <textarea matInput rows="6" formControlName="description"></textarea>
            </mat-form-field>
          </form>

          <div class="info-card" style="margin-top:1rem;">
            <div class="section-head">
              <div>
                <div class="eyebrow">Media upload</div>
                <h3 style="margin-top:.35rem;">Primary image</h3>
              </div>
            </div>
            <input type="file" accept="image/*" (change)="pick($event)">
            <div class="muted" style="margin-top:.5rem;">A preview appears immediately so merchandisers can confirm the asset before saving.</div>
          </div>

          <div class="card-actions">
            <button mat-flat-button color="primary" type="button" (click)="save()">{{ editMode ? 'Save Changes' : 'Create Product' }}</button>
            <button mat-stroked-button color="primary" type="button" (click)="validateAi()">AI Validate</button>
            <button mat-stroked-button color="accent" type="button" (click)="predictAi()">Predict Approval</button>
          </div>
        </mat-card>

        <div class="page-shell">
          <div class="panel" style="padding: 1.25rem;">
            <div class="section-head">
              <div>
                <div class="eyebrow">Live preview</div>
                <h3 style="margin-top:.35rem;">Readiness summary</h3>
              </div>
              <strong>{{ readinessScore }}%</strong>
            </div>

            <div class="value-grid">
              <div class="value-tile">
                Price after discount
                <strong>{{ discountedPrice | number:'1.0-0' }}</strong>
              </div>
              <div class="value-tile">
                Stock posture
                <strong>{{ stockLabel }}</strong>
              </div>
              <div class="value-tile">
                Description depth
                <strong>{{ descriptionLength }} chars</strong>
              </div>
            </div>

            <div style="margin-top:1rem;" *ngIf="filePreview; else uploadHint">
              <img class="preview-image" [src]="filePreview" alt="Selected preview">
            </div>
          </div>

          <div class="panel" style="padding: 1.25rem;">
            <div class="section-head">
              <div>
                <div class="eyebrow">Checklist</div>
                <h3 style="margin-top:.35rem;">Before sending to review</h3>
              </div>
            </div>
            <div class="timeline">
              <div class="split-line">
                <div>
                  <strong>Name and SKU</strong>
                  <div class="muted">Basic catalog identity is present.</div>
                </div>
                <span>{{ form.value.name && form.value.sku ? 'Ready' : 'Missing' }}</span>
              </div>
              <div class="split-line">
                <div>
                  <strong>Description quality</strong>
                  <div class="muted">Longer, clearer descriptions improve AI checks.</div>
                </div>
                <span>{{ descriptionLength >= 40 ? 'Strong' : 'Needs work' }}</span>
              </div>
              <div class="split-line">
                <div>
                  <strong>Commercial fields</strong>
                  <div class="muted">Price, discount, and inventory are set.</div>
                </div>
                <span>{{ hasCommercialData ? 'Ready' : 'Incomplete' }}</span>
              </div>
              <div class="split-line">
                <div>
                  <strong>Media coverage</strong>
                  <div class="muted">At least one image helps approval readiness.</div>
                </div>
                <span>{{ file || editMode ? 'Covered' : 'Add media' }}</span>
              </div>
            </div>
          </div>

          <div class="panel" style="padding: 1.25rem;">
            <div class="section-head">
              <div>
                <div class="eyebrow">AI output</div>
                <h3 style="margin-top:.35rem;">Assistant response</h3>
              </div>
            </div>
            <pre class="prettified-json">{{ ai | json }}</pre>
          </div>
        </div>
      </section>
    </div>

    <ng-template #uploadHint>
      <div class="empty-state" style="margin-top:1rem;">
        <strong>No image selected yet.</strong>
        <div style="margin-top:.4rem;">Upload a primary asset to make the preview feel complete.</div>
      </div>
    </ng-template>
  `
})
export class ProductFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(ProductService);
  private readonly workflow = inject(WorkflowService);
  private readonly aiService = inject(AiService);
  private readonly toast = inject(ToastService);

  file: File | null = null;
  filePreview: string | null = null;
  productId: string | null = null;
  ai: unknown;

  readonly form = this.fb.group({
    name: ['', Validators.required],
    sku: ['', Validators.required],
    categoryId: ['11111111-1111-1111-1111-111111111111', Validators.required],
    description: ['', [Validators.required, Validators.minLength(10)]],
    price: [1000, Validators.required],
    discount: [0, Validators.required],
    stock: [10, Validators.required]
  });

  get editMode(): boolean {
    return !!this.productId;
  }

  get descriptionLength(): number {
    return this.form.value.description?.length ?? 0;
  }

  get discountedPrice(): number {
    const price = Number(this.form.value.price ?? 0);
    const discount = Number(this.form.value.discount ?? 0);
    return Math.max(0, price - discount);
  }

  get stockLabel(): string {
    const stock = Number(this.form.value.stock ?? 0);
    if (stock > 20) return 'Healthy';
    if (stock > 0) return 'Low';
    return 'Out';
  }

  get hasCommercialData(): boolean {
    return Number(this.form.value.price ?? 0) > 0 && Number(this.form.value.stock ?? 0) >= 0;
  }

  get readinessScore(): number {
    const checks = [
      !!this.form.value.name,
      !!this.form.value.sku,
      this.descriptionLength >= 40,
      this.hasCommercialData,
      !!this.file || this.editMode
    ];
    return Math.round((checks.filter(Boolean).length / checks.length) * 100);
  }

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id');
    if (!this.productId) return;

    this.service.get(this.productId).subscribe((product) => this.form.patchValue({
      name: product.name,
      sku: product.sku,
      description: product.description ?? product.name
    }));
  }

  pick(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.file = input.files?.[0] ?? null;
    if (!this.file) {
      this.filePreview = null;
      return;
    }

    const reader = new FileReader();
    reader.onload = () => this.filePreview = String(reader.result ?? '');
    reader.readAsDataURL(this.file);
  }

  save(): void {
    if (this.form.invalid) return;

    const value = this.form.getRawValue();
    if (!this.editMode) {
      this.service.create({
        name: value.name!,
        sku: value.sku!,
        categoryId: value.categoryId!,
        description: value.description!
      }).subscribe({
        next: () => {
          this.toast.success('Product created');
          this.router.navigate(['/products']);
        },
        error: () => this.toast.error('Create failed')
      });
      return;
    }

    this.service.update(this.productId!, {
      name: value.name!,
      description: value.description!
    }).subscribe({
      next: () => {
        this.workflow.updatePricing(this.productId!, {
          price: Number(value.price),
          discount: Number(value.discount)
        }).subscribe();
        this.workflow.updateInventory(this.productId!, {
          stock: Number(value.stock)
        }).subscribe();
        if (this.file) this.service.upload(this.productId!, this.file).subscribe();
        this.toast.success('Product updated');
        this.router.navigate(['/products', this.productId]);
      },
      error: () => this.toast.error('Update failed')
    });
  }

  validateAi(): void {
    const value = this.form.getRawValue();
    this.aiService.validate({
      description: value.description,
      imageCount: this.file ? 1 : 0,
      price: value.price,
      category: 'General'
    }).subscribe((result) => this.ai = result);
  }

  predictAi(): void {
    const value = this.form.getRawValue();
    this.aiService.predict({
      description: value.description,
      imageCount: this.file ? 1 : 0,
      price: value.price,
      category: 'General'
    }).subscribe((result) => this.ai = result);
  }
}

@NgModule({
  declarations: [ProductListComponent, ProductDetailComponent, ProductFormComponent],
  imports: [CommonModule, ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatIconModule, MatInputModule, MatSelectModule, RouterModule.forChild([
    { path: '', component: ProductListComponent },
    { path: 'new', component: ProductFormComponent },
    { path: ':id/edit', component: ProductFormComponent },
    { path: ':id', component: ProductDetailComponent }
  ])]
})
export class ProductsModule {}
