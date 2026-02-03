import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environment/environment';
export const baseUrlInterceptor: HttpInterceptorFn = (req, next) => {

  const baseUrl = environment.apiBaseUrl;

  if (req.url.startsWith('http')) {
    return next(req);
  }

  const apiReq = req.clone({
    url: `${baseUrl}${req.url}`
  });
  return next(apiReq);
};