import { Routes } from '@angular/router';
import { LoginComponent } from './login/login';
import { AuthCallback } from './auth-calback/auth-calback';
import { Home } from './home/home';
import { CompleteProfileComponent } from './complete-profile/complete-profile';

export const routes: Routes = [

    // DEFAULT LOGIN PAGE
  {
    path: '',
    component: LoginComponent,
    pathMatch: 'full'
  },

  // AUTH CALLBACK
  {
    path: 'gateway/auth/callback',
    component: AuthCallback
  },

  {
    path: 'home',
    component: Home
  },
  {
    path: 'complete-profile',
    component: CompleteProfileComponent
  }
];
