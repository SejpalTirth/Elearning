import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../../../service/auth-service';
import { AuthStateService } from '../../../service/auth-state-service';
import { AuthUser } from '../../../modules/auth/auth-user.model';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class HeaderComponent {

  private readonly auth = inject(AuthService);
  private readonly authState = inject(AuthStateService);

  user$: Observable<AuthUser | null> = this.authState.user$;

  dropdownOpen = false;

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