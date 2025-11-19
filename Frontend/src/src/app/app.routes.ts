import { Routes } from '@angular/router';

import { LoginComponent } from './GatewayService/login/login';
import { AuthCallback } from './GatewayService/auth-callback/auth-callback';

export const routes: Routes = [
  // OAuth callback route MUST be above the wildcard
  { path: 'gateway/auth/callback', component: AuthCallback },

  // Normal routes
  { path: '', component: LoginComponent },

  // Wildcard redirect (keep LAST)
  { path: '**', redirectTo: '' }
];
