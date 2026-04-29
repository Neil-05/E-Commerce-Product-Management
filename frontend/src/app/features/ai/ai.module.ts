import { NgModule, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AiService } from '../../core/services/ai.service';

@Component({
  selector: 'app-ai-page',
  standalone: false,
  template: `
    <div class="page-shell">
      <section class="hero-grid">
        <div class="hero-panel">
          <div class="eyebrow">AI Workspace</div>
          <h2>Run quality checks, approval forecasts, and assistant prompts from a richer command deck.</h2>
          <p class="muted" style="color: rgba(239, 246, 255, 0.82);">This turns the AI page from a plain test form into an actual operating surface for merchandisers and reviewers.</p>
        </div>

        <div class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Quick prompts</div>
              <h3 style="margin-top:.35rem;">Preset actions</h3>
            </div>
          </div>
          <div class="action-grid">
            <button mat-stroked-button type="button" (click)="applyPreset('Electronics', 50000, 3, 'High-end smartphone with all-day battery life, flagship camera, and premium finish.')">Electronics</button>
            <button mat-stroked-button type="button" (click)="applyPreset('Fashion', 3500, 2, 'Minimal cotton overshirt with a structured silhouette and everyday versatility.')">Fashion</button>
            <button mat-stroked-button type="button" (click)="applyPreset('Home', 12999, 4, 'Compact air purifier with HEPA filter, quiet night mode, and low maintenance design.')">Home</button>
          </div>
        </div>
      </section>

      <section class="grid-2">
        <mat-card class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Analysis input</div>
              <h3 style="margin-top:.35rem;">Listing evaluation</h3>
            </div>
          </div>
          <form [formGroup]="analysis" class="form-grid">
            <mat-form-field class="span-2">
              <mat-label>Description</mat-label>
              <textarea matInput rows="6" formControlName="description"></textarea>
            </mat-form-field>
            <mat-form-field>
              <mat-label>Image Count</mat-label>
              <input matInput type="number" formControlName="imageCount">
            </mat-form-field>
            <mat-form-field>
              <mat-label>Price</mat-label>
              <input matInput type="number" formControlName="price">
            </mat-form-field>
            <mat-form-field>
              <mat-label>Category</mat-label>
              <input matInput formControlName="category">
            </mat-form-field>
          </form>

          <div class="card-actions">
            <button mat-flat-button color="primary" type="button" (click)="validate()">Validate</button>
            <button mat-stroked-button color="primary" type="button" (click)="predict()">Predict</button>
            <button mat-stroked-button color="accent" type="button" (click)="feedback()">Feedback</button>
          </div>

          <div class="value-grid" style="margin-top:1rem;">
            <div class="value-tile">
              Description size
              <strong>{{ analysis.value.description?.length || 0 }} chars</strong>
            </div>
            <div class="value-tile">
              Media coverage
              <strong>{{ analysis.value.imageCount || 0 }} images</strong>
            </div>
            <div class="value-tile">
              Price band
              <strong>{{ priceBand }}</strong>
            </div>
          </div>
        </mat-card>

        <mat-card class="panel" style="padding: 1.25rem;">
          <div class="section-head">
            <div>
              <div class="eyebrow">Assistant chat</div>
              <h3 style="margin-top:.35rem;">Ask for guidance</h3>
            </div>
          </div>
          <form [formGroup]="chatForm">
            <mat-form-field style="width: 100%;">
              <mat-label>Ask the assistant</mat-label>
              <textarea matInput rows="5" formControlName="message"></textarea>
            </mat-form-field>
            <div class="card-actions">
              <button mat-flat-button color="primary" type="button" (click)="chat()">Send</button>
            </div>
          </form>
          <pre class="prettified-json">{{ result | json }}</pre>
        </mat-card>
      </section>
    </div>
  `
})
export class AiComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AiService);

  readonly analysis = this.fb.group({
    description: ['Great smartphone with battery life', Validators.required],
    imageCount: [3, Validators.required],
    price: [50000, Validators.required],
    category: ['Electronics', Validators.required]
  });

  readonly chatForm = this.fb.group({
    message: ['Validate a smartphone listing with good camera and 3 images', Validators.required]
  });

  result: unknown;

  get priceBand(): string {
    const price = Number(this.analysis.value.price ?? 0);
    if (price >= 30000) return 'Premium';
    if (price >= 10000) return 'Mid-market';
    return 'Value';
  }

  applyPreset(category: string, price: number, imageCount: number, description: string): void {
    this.analysis.patchValue({ category, price, imageCount, description });
  }

  validate(): void {
    this.service.validate(this.analysis.getRawValue()).subscribe((value) => this.result = value);
  }

  predict(): void {
    this.service.predict(this.analysis.getRawValue()).subscribe((value) => this.result = value);
  }

  feedback(): void {
    this.service.feedback(this.analysis.getRawValue()).subscribe((value) => this.result = value);
  }

  chat(): void {
    this.service.chat(this.chatForm.getRawValue()).subscribe((value) => this.result = value);
  }
}

@NgModule({
  declarations: [AiComponent],
  imports: [CommonModule, ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, RouterModule.forChild([{ path: '', component: AiComponent }])]
})
export class AiModule {}
