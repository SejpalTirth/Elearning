import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard {

  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  canActivate(): boolean {
    const access = this.auth.getAccessToken();
    const refresh = this.auth.getRefreshToken();

    // Only block if BOTH tokens missing → user truly logged out
    if (!access && !refresh) {
      this.router.navigate(['/login']);
      return false;
    }

    return true;
  }
}
