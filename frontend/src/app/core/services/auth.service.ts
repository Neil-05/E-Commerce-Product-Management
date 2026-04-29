import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, UserProfile } from '../models/api.models';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'epms_token';
  private readonly userKey = 'epms_user';
  private readonly userSubject = new BehaviorSubject<UserProfile | null>(this.readUser());
  readonly user$ = this.userSubject.asObservable();

  constructor(private readonly api: ApiService) {}

  login(payload: { email: string; password: string }): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/gateway/auth/login', payload).pipe(tap((res) => this.setSession(res)));
  }

  register(payload: { name: string; email: string; password: string; role: string }): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/gateway/auth/register', payload).pipe(tap((res) => this.setSession(res)));
  }

  forgotPassword(payload: { email: string }): Observable<string> {
    return this.api.post<string>('/gateway/auth/forgot-password', payload);
  }

  resetPassword(payload: { email: string; otp: string; newPassword: string }): Observable<string> {
    return this.api.post<string>('/gateway/auth/reset-password', payload);
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.userSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getUser(): UserProfile | null {
    return this.userSubject.value;
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.includes(this.getUser()?.role ?? '');
  }

  private setSession(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    const user = this.decodeToken(response.token) ?? { email: response.email, role: response.role };
    localStorage.setItem(this.userKey, JSON.stringify(user));
    this.userSubject.next(user);
  }

  private readUser(): UserProfile | null {
    const raw = localStorage.getItem(this.userKey);
    return raw ? (JSON.parse(raw) as UserProfile) : null;
  }

  private decodeToken(token: string): UserProfile | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return {
        email: payload.email ?? payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ?? '',
        role: payload.role ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? '',
        name: payload.name ?? payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? ''
      };
    } catch {
      return null;
    }
  }
}
