import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection
} from '@angular/core';
import {
  provideHttpClient,
  withInterceptors
} from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { environment } from '../../environments/environment';

import { API_BASE_URL, credentialsInterceptor } from '@frontend/core';
import { baseUrlInterceptor } from '@frontend/core';
import { authInterceptor } from '@frontend/auth';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),

    // SINGLE interceptor pipeline
    provideHttpClient(
      withInterceptors([
        baseUrlInterceptor,
        authInterceptor,
        credentialsInterceptor
      ])
    ),

    // API base URL
    {
      provide: API_BASE_URL,
      useValue: environment.apiBaseUrl
    }
  ]
};
