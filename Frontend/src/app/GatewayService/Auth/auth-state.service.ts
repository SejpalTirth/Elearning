import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from 'Environment/environment';
import { AuthUser } from './auth-user.model';

type AuthStatus = 'idle' | 'loading' | 'authenticated';

@Injectable({ providedIn: 'root' })
export class AuthStateService {

  private readonly meUrl = `${environment.baseapiurl}/GatewayAuth/me`;

  private readonly userSubject = new BehaviorSubject<AuthUser | null>(null);
  user$ = this.userSubject.asObservable();

  private readonly statusSubject =
    new BehaviorSubject<AuthStatus>('idle');
  status$ = this.statusSubject.asObservable();

  // Session guard (kills race conditions)
  private sessionId = 0;

  constructor(private http: HttpClient) {}

  /** Call ONCE on app startup */
  initialize(): void {
    if (!this.hasToken()) {
      this.clear();
      return;
    }

    this.loadUserInternal();
  }

  /** Call AFTER login success */
  onLoginSuccess(): void {
    this.loadUserInternal();
  }

  /** INTERNAL guarded loader */
  private loadUserInternal(): void {
    const currentSession = ++this.sessionId;
    this.statusSubject.next('loading');

    this.http.get<AuthUser>(this.meUrl).subscribe({
      next: user => {
        if (currentSession !== this.sessionId) {return;}

        this.userSubject.next(user);
        this.statusSubject.next('authenticated');
      },
      error: () => {
        if (currentSession !== this.sessionId) {return;}
        this.clear();
      }
    });
  }

  /** Hard reset (logout / token failure) */
  clear(): void {
    this.sessionId++; // Invalidate inflight /me
    this.userSubject.next(null);
    this.statusSubject.next('idle');
  }

  /** Helpers */
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

  private hasToken(): boolean {
    return !!localStorage.getItem('accessToken'); 
  }
}
