import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
  APP_INITIALIZER
} from '@angular/core';
import {
  provideHttpClient,
  withInterceptors
} from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { AuthStateService } from './service/auth-state-service';
import { authInterceptor } from './middleware/auth.interceptor';
import { baseUrlInterceptor } from './middleware/base-url.interceptor';
import { credentialsInterceptor } from './middleware/credentials.interceptor';

function initAuth(authState: AuthStateService) {
  return () => authState.initialize();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),

    provideHttpClient(
      withInterceptors([
        baseUrlInterceptor,
        authInterceptor,
        credentialsInterceptor
      ])
    ),

    {
      provide: APP_INITIALIZER,
      useFactory: initAuth,
      deps: [AuthStateService],
      multi: true
    }
  ]
};
