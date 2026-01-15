import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';
import { AuthUser } from '../models/auth-user.model';

type AuthStatus = 'idle' | 'loading' | 'authenticated';

@Injectable({ providedIn: 'root' })
export class AuthStateService {

  private readonly userSubject = new BehaviorSubject<AuthUser | null>(null);
  user$ = this.userSubject.asObservable();

  private readonly statusSubject = new BehaviorSubject<AuthStatus>('idle');
  status$ = this.statusSubject.asObservable();

  private sessionId = 0;

  constructor(private auth: AuthService) {}

  initialize(): void {
    this.loadUser();
  }

  onLoginSuccess(): void {
    this.loadUser();
  }

  private loadUser(): void {
    const currentSession = ++this.sessionId;
    this.statusSubject.next('loading');

    this.auth.getMe().subscribe({
      next: user => {
        if (currentSession !== this.sessionId) {return;}

        this.userSubject.next(user);
        this.statusSubject.next('authenticated');
      },
      error: () => {
        if (currentSession !== this.sessionId) {return;}

        // DO NOT auto-logout here
        this.statusSubject.next('idle');
      }
    });
  }

  clear(): void {
    this.sessionId++;
    this.userSubject.next(null);
    this.statusSubject.next('idle');
  }

  // ----------------- helpers -----------------

  get user(): AuthUser | null {
    return this.userSubject.value;
  }

  get isLoggedIn(): boolean {
    return this.statusSubject.value === 'authenticated';
  }

  get role(): string | null {
    return this.userSubject.value?.role ?? null;
  }

  get userId(): string | null {
    return this.userSubject.value?.userId ?? null;
  }
}
