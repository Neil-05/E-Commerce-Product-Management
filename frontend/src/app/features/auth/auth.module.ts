import { NgModule, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: false,
  template: `<section style="min-height:100vh;display:grid;place-items:center;padding:2rem"><mat-card class="panel" style="width:min(100%,420px);padding:1rem"><div class="eyebrow">Secure Access</div><h1>Login</h1><form [formGroup]="form" (ngSubmit)="submit()" class="form-grid"><mat-form-field><mat-label>Email</mat-label><input matInput formControlName="email"></mat-form-field><mat-form-field><mat-label>Password</mat-label><input matInput type="password" formControlName="password"></mat-form-field><button mat-flat-button color="primary">Login</button><a routerLink="/auth/forgot-password">Forgot password?</a><a routerLink="/auth/register">Create account</a></form></mat-card></section>`
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  readonly form = this.fb.group({ email: ['', [Validators.required, Validators.email]], password: ['', Validators.required] });
  submit(): void { if (this.form.invalid) return; this.auth.login(this.form.getRawValue() as any).subscribe({ next: () => this.router.navigate(['/products']), error: () => this.toast.error('Login failed') }); }
}

@Component({
  selector: 'app-register',
  standalone: false,
  template: `<section style="min-height:100vh;display:grid;place-items:center;padding:2rem"><mat-card class="panel" style="width:min(100%,460px);padding:1rem"><div class="eyebrow">Onboarding</div><h1>Register</h1><form [formGroup]="form" (ngSubmit)="submit()" class="form-grid"><mat-form-field><mat-label>Name</mat-label><input matInput formControlName="name"></mat-form-field><mat-form-field><mat-label>Email</mat-label><input matInput formControlName="email"></mat-form-field><mat-form-field><mat-label>Password</mat-label><input matInput type="password" formControlName="password"></mat-form-field><mat-form-field><mat-label>Role</mat-label><mat-select formControlName="role"><mat-option *ngFor="let role of roles" [value]="role">{{ role }}</mat-option></mat-select></mat-form-field><button mat-flat-button color="primary">Register</button><a routerLink="/auth/login">Back to login</a></form></mat-card></section>`
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  readonly roles = ['Admin', 'ProductManager', 'ContentExecutive', 'User'];
  readonly form = this.fb.group({ name: ['', Validators.required], email: ['', [Validators.required, Validators.email]], password: ['', [Validators.required, Validators.minLength(6)]], role: ['User', Validators.required] });
  submit(): void { if (this.form.invalid) return; this.auth.register(this.form.getRawValue() as any).subscribe({ next: () => this.router.navigate(['/products']), error: () => this.toast.error('Registration failed. Backend may only allow Admin/User.') }); }
}

@Component({
  selector: 'app-forgot-password',
  standalone: false,
  template: `<section style="min-height:100vh;display:grid;place-items:center;padding:2rem"><mat-card class="panel" style="width:min(100%,420px);padding:1rem"><h1>Forgot Password</h1><form [formGroup]="form" (ngSubmit)="submit()" class="form-grid"><mat-form-field><mat-label>Email</mat-label><input matInput formControlName="email"></mat-form-field><button mat-flat-button color="primary">Send OTP</button><a routerLink="/auth/login">Back to login</a></form></mat-card></section>`
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  readonly form = this.fb.group({ email: ['', [Validators.required, Validators.email]] });
  submit(): void { if (this.form.invalid) return; this.auth.forgotPassword(this.form.getRawValue() as any).subscribe({ next: () => { this.toast.success('OTP sent'); this.router.navigate(['/auth/reset-password']); }, error: () => this.toast.error('Unable to send OTP') }); }
}

@Component({
  selector: 'app-reset-password',
  standalone: false,
  template: `<section style="min-height:100vh;display:grid;place-items:center;padding:2rem"><mat-card class="panel" style="width:min(100%,420px);padding:1rem"><h1>Reset Password</h1><form [formGroup]="form" (ngSubmit)="submit()" class="form-grid"><mat-form-field><mat-label>Email</mat-label><input matInput formControlName="email"></mat-form-field><mat-form-field><mat-label>OTP</mat-label><input matInput formControlName="otp"></mat-form-field><mat-form-field><mat-label>New Password</mat-label><input matInput type="password" formControlName="newPassword"></mat-form-field><button mat-flat-button color="primary">Reset</button></form></mat-card></section>`
})
export class ResetPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  readonly form = this.fb.group({ email: ['', [Validators.required, Validators.email]], otp: ['', Validators.required], newPassword: ['', [Validators.required, Validators.minLength(6)]] });
  submit(): void { if (this.form.invalid) return; this.auth.resetPassword(this.form.getRawValue() as any).subscribe({ next: () => { this.toast.success('Password reset'); this.router.navigate(['/auth/login']); }, error: () => this.toast.error('Reset failed') }); }
}

@NgModule({
  declarations: [LoginComponent, RegisterComponent, ForgotPasswordComponent, ResetPasswordComponent],
  imports: [CommonModule, ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, RouterModule.forChild([
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'forgot-password', component: ForgotPasswordComponent },
    { path: 'reset-password', component: ResetPasswordComponent },
    { path: '', pathMatch: 'full', redirectTo: 'login' }
  ])]
})
export class AuthModule {}
