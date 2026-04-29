import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';
import { LayoutComponent } from './shared/components/layout.component';

const routes: Routes = [
  { path: 'auth', loadChildren: () => import('./features/auth/auth.module').then((m) => m.AuthModule) },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: 'products', loadChildren: () => import('./features/products/products.module').then((m) => m.ProductsModule) },
      { path: 'workflow', loadChildren: () => import('./features/workflow/workflow.module').then((m) => m.WorkflowModule) },
      { path: 'admin', canActivate: [RoleGuard], data: { roles: ['Admin'] }, loadChildren: () => import('./features/admin/admin.module').then((m) => m.AdminModule) },
      { path: 'ai', loadChildren: () => import('./features/ai/ai.module').then((m) => m.AiModule) },
      { path: '', pathMatch: 'full', redirectTo: 'products' }
    ]
  },
  { path: '**', redirectTo: 'auth/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
