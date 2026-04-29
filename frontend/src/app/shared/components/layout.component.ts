import { Component, HostListener, inject } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { LoaderService } from '../../core/services/loader.service';

interface NavItem {
  label: string;
  route: string;
  description: string;
  roles?: string[];
}

@Component({
  selector: 'app-layout',
  standalone: false,
  template: `
    <div class="loader-pill" *ngIf="(loader.loading$ | async) && (loader.loading$ | async)! > 0">Syncing workspace...</div>
    <mat-sidenav-container class="workspace-shell">
      <mat-sidenav [mode]="isMobile ? 'over' : 'side'" [opened]="!isMobile || mobileOpen" class="workspace-sidenav" (closedStart)="mobileOpen = false">
        <div class="sidenav-inner">
          <div class="brand-card">
            <div class="eyebrow">Commerce OS</div>
            <h2>AI Product Control Center</h2>
            <p>Catalog operations, approval flow, admin health, and AI support in one place.</p>
            <div class="pill-row">
              <span class="soft-chip">Live workflows</span>
              <span class="soft-chip">Ops visibility</span>
            </div>
          </div>

          <div class="nav-block">
            <div class="nav-label">Workspace</div>
            <a
              *ngFor="let item of visibleNavItems"
              class="nav-item"
              [class.active]="isActive(item.route)"
              [routerLink]="item.route"
              (click)="closeMobileNav()"
            >
              <div>{{ item.label }}</div>
              <small>{{ item.description }}</small>
            </a>
          </div>

          <div class="sidebar-foot">
            <div class="info-card sidebar-profile">
              <div class="muted">Signed in as</div>
              <strong>{{ auth.getUser()?.name || auth.getUser()?.email }}</strong>
              <div class="status-pill" [ngClass]="roleClass">{{ auth.getUser()?.role || 'User' }}</div>
            </div>
          </div>
        </div>
      </mat-sidenav>

      <mat-sidenav-content>
        <header class="workspace-topbar">
          <button mat-icon-button type="button" class="menu-toggle" (click)="mobileOpen = !mobileOpen">
            <mat-icon>menu</mat-icon>
          </button>

          <div>
            <div class="eyebrow">Operations Hub</div>
            <h1>{{ pageTitle }}</h1>
            <div class="muted">Focused on {{ pageSubtitle }}</div>
          </div>

          <div class="topbar-actions">
            <div class="muted desktop-only">{{ todayLabel }}</div>
            <button mat-stroked-button type="button" (click)="logout()">Logout</button>
          </div>
        </header>

        <main class="workspace-content">
          <router-outlet></router-outlet>
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .workspace-shell {
      min-height: 100vh;
      background: transparent;
    }

    .workspace-sidenav {
      width: 310px;
      border-right: 1px solid rgba(148, 163, 184, 0.18);
      background:
        linear-gradient(180deg, rgba(8, 20, 43, 0.96), rgba(13, 42, 92, 0.94)),
        #0f172a;
      color: #eff6ff;
    }

    .sidenav-inner {
      display: flex;
      flex-direction: column;
      min-height: 100%;
      padding: 1.2rem;
      gap: 1.2rem;
    }

    .brand-card {
      padding: 1.25rem;
      border-radius: 24px;
      background: linear-gradient(180deg, rgba(255, 255, 255, 0.12), rgba(255, 255, 255, 0.05));
      border: 1px solid rgba(255, 255, 255, 0.12);
    }

    .brand-card h2 {
      margin: 0.35rem 0 0.6rem;
      line-height: 1.1;
    }

    .brand-card p {
      margin: 0 0 1rem;
      color: rgba(226, 232, 240, 0.82);
    }

    .nav-block {
      display: grid;
      gap: 0.65rem;
    }

    .nav-label {
      color: rgba(226, 232, 240, 0.6);
      text-transform: uppercase;
      letter-spacing: 0.12em;
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0 0.4rem;
    }

    .nav-item {
      display: block;
      padding: 0.9rem 1rem;
      border-radius: 18px;
      border: 1px solid transparent;
      background: rgba(255, 255, 255, 0.03);
      transition: 160ms ease;
    }

    .nav-item small {
      display: block;
      margin-top: 0.2rem;
      color: rgba(226, 232, 240, 0.66);
    }

    .nav-item:hover,
    .nav-item.active {
      transform: translateX(2px);
      border-color: rgba(255, 255, 255, 0.16);
      background: rgba(255, 255, 255, 0.1);
    }

    .sidebar-foot {
      margin-top: auto;
    }

    .sidebar-profile {
      background: rgba(255, 255, 255, 0.08);
      color: white;
    }

    .sidebar-profile .status-pill {
      width: fit-content;
      margin-top: 0.7rem;
      background: rgba(255, 255, 255, 0.08);
    }

    .workspace-topbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1rem;
      padding: 1.2rem 1.4rem 0.4rem;
    }

    .workspace-topbar h1 {
      margin: 0.3rem 0 0;
      font-size: clamp(1.5rem, 3vw, 2.1rem);
    }

    .topbar-actions {
      display: flex;
      align-items: center;
      gap: 0.8rem;
    }

    .menu-toggle {
      display: none;
    }

    .workspace-content {
      padding: 0.8rem 1.4rem 1.6rem;
    }

    @media (max-width: 960px) {
      .workspace-topbar {
        padding-top: 1rem;
      }

      .menu-toggle {
        display: inline-flex;
      }
    }

    @media (max-width: 720px) {
      .workspace-topbar {
        align-items: flex-start;
        flex-wrap: wrap;
      }

      .desktop-only {
        display: none;
      }

      .workspace-content {
        padding-inline: 1rem;
      }
    }
  `]
})
export class LayoutComponent {
  readonly auth = inject(AuthService);
  readonly loader = inject(LoaderService);
  private readonly router = inject(Router);
  mobileOpen = false;
  isMobile = typeof window !== 'undefined' ? window.innerWidth < 960 : false;

  readonly navItems: NavItem[] = [
    { label: 'Products', route: '/products', description: 'Catalog overview and creation tools' },
    { label: 'Workflow', route: '/workflow', description: 'Review queues and approval actions' },
    { label: 'Admin', route: '/admin', description: 'Metrics and audit visibility', roles: ['Admin'] },
    { label: 'AI', route: '/ai', description: 'Validation, forecasting, and assistant tasks' }
  ];

  constructor() {
    this.router.events.pipe(filter((event) => event instanceof NavigationEnd)).subscribe(() => this.closeMobileNav());
  }

  get visibleNavItems(): NavItem[] {
    return this.navItems.filter((item) => !item.roles || this.auth.hasAnyRole(item.roles));
  }

  get pageTitle(): string {
    const url = this.router.url;
    if (url.startsWith('/products')) return 'Product workspace';
    if (url.startsWith('/workflow')) return 'Workflow monitor';
    if (url.startsWith('/admin')) return 'Admin insights';
    if (url.startsWith('/ai')) return 'AI command deck';
    return 'Workspace';
  }

  get pageSubtitle(): string {
    const url = this.router.url;
    if (url.startsWith('/products')) return 'catalog quality, pricing readiness, and launch prep';
    if (url.startsWith('/workflow')) return 'review throughput and publishing flow';
    if (url.startsWith('/admin')) return 'business health and audit traceability';
    if (url.startsWith('/ai')) return 'model assistance and listing guidance';
    return 'day-to-day product operations';
  }

  get roleClass(): string {
    return this.auth.hasAnyRole(['Admin']) ? 'status-published' : 'status-approved';
  }

  get todayLabel(): string {
    return new Intl.DateTimeFormat('en-US', { weekday: 'long', month: 'short', day: 'numeric' }).format(new Date());
  }

  isActive(route: string): boolean {
    return this.router.url.startsWith(route);
  }

  closeMobileNav(): void {
    if (this.isMobile) this.mobileOpen = false;
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }

  @HostListener('window:resize')
  onResize(): void {
    this.isMobile = window.innerWidth < 960;
    if (!this.isMobile) this.mobileOpen = false;
  }
}
