import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { API_BASE_URL } from '../tokens/api-base-url.token';

export const baseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  const baseUrl = inject(API_BASE_URL);

  // Ignore absolute URLs (OAuth redirects etc.)
  if (req.url.startsWith('http')) {
    return next(req);
  }

  const apiReq = req.clone({
    url: `${baseUrl}${req.url}`
  });
  return next(apiReq);
};
