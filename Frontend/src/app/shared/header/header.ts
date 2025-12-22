import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';

import { AuthService } from 'app/GatewayService/Auth/auth.service';
import { AuthStateService } from 'app/GatewayService/Auth/auth-state.service';
import { AuthUser } from '../../GatewayService/Auth/auth-user.model';

@Component({
  selector: 'app-shared-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class SharedHeaderComponent {

  private readonly auth = inject(AuthService);
  private readonly authState = inject(AuthStateService);

  /** Reactive user stream */
  user$: Observable<AuthUser | null> = this.authState.user$;

  /** UI state */
  dropdownOpen = false;

  /** Derived helpers (safe & simple) */
  get loggedIn(): boolean {
    return this.authState.isLoggedIn;
  }

  get userName(): string {
    return (
      this.authState.user?.name ||
      this.authState.user?.email ||
      'User'
    );
  }

  get role(): string | null {
    return this.authState.role;
  }

  toggleDropdown(): void {
    this.dropdownOpen = !this.dropdownOpen;
  }

  logout(): void {
    this.dropdownOpen = false;
    this.auth.logout();
    this.authState.clear();
  }
}
