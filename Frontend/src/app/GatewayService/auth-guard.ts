import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard {

  constructor(private auth: AuthService, private router: Router) {}

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
