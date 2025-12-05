import { ApplicationConfig, inject } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { AuthInterceptor } from './GatewayService/auth.interceptor';
import { HttpRequest, HttpHandler } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [

    AuthInterceptor,
    provideRouter(routes),

    provideHttpClient(
      withInterceptors([
        (req, nextFn) => {

          const interceptor = inject(AuthInterceptor);

          // Convert nextFn → HttpHandler
          const handler: HttpHandler = {
            handle: (modifiedReq: HttpRequest<any>) => nextFn(modifiedReq)
          };

          return interceptor.intercept(req, handler);
        }
      ])
    )
  ]
};
