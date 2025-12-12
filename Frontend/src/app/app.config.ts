import { ApplicationConfig, inject } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { AuthInterceptor } from './GatewayService/Auth/auth.interceptor';
import { HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

export const appConfig: ApplicationConfig = {
  providers: [

    AuthInterceptor,
    provideRouter(routes),

    provideHttpClient(
      withInterceptors([
        (req, nextFn): Observable<HttpEvent<any>> => {

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
