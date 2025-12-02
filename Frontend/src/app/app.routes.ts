import { Routes } from '@angular/router';

import { LoginComponent } from './GatewayService/login/login';
import { AuthCallback } from './GatewayService/auth-callback/auth-callback';
import { Home } from './GatewayService/home/home';
import { AuthGuard } from './GatewayService/auth-guard';

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

  // COMPLETE PROFILE PAGE
  {
    path: 'complete-profile',
    loadComponent: () =>
      import('./GatewayService/complete-profile/complete-profile')
        .then(m => m.CompleteProfileComponent)
  },

  // SECURED AREA
  {
    path: '',
    canActivate: [AuthGuard],
    children: [

      // HOME
      {
        path: 'home',
        component: Home
      },

      // COURSES MODULE
      {
        path: 'courses',
        loadChildren: () =>
          import('./CourseService/course.routes')
            .then(m => m.COURSE_ROUTES)
      },

      // NEW: ASSESSMENT MODULE
      {
        path: 'assessment',
        loadChildren: () =>
          import('./AssessmentService/assessment.routes')
            .then(m => m.ASSESSMENT_ROUTES)
      },

      {
      path: 'manage-users',
      loadChildren: () =>
        import('./UserService/user.routes').then(m => m.USER_ROUTES)
      }

    ]
  },

  // FALLBACK
  { path: '**', redirectTo: '' }
];
