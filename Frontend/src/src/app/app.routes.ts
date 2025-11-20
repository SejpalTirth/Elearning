import { Routes } from '@angular/router';

import { LoginComponent } from './GatewayService/login/login';
import { AuthCallback } from './GatewayService/auth-callback/auth-callback';
import { Home } from './GatewayService/home/home';
import { AuthGuard } from './GatewayService/auth-guard';

export const routes: Routes = [

  // FIXED: Correct callback route with pathMatch: 'full'
  {
    path: 'gateway/auth/callback',
    component: AuthCallback,
    pathMatch: 'full'
  },

  // Protected home route (only one!)
  {
    path: 'home',
    component: Home,
    canActivate: [AuthGuard]
  },

  // Complete Profile page
  {
    path: 'complete-profile',
    loadComponent: () =>
      import('./GatewayService/complete-profile/complete-profile')
        .then(m => m.CompleteProfileComponent)
  },

  // Login page
  { path: '', component: LoginComponent },

  // Redirect unknown paths
  { path: '**', redirectTo: '' }
];
