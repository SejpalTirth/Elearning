import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { AuthStateService, AuthService } from '@frontend/auth';
import { MenuItem } from './menu.model';

@Component({
  selector: 'app-shared-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class SharedHeaderComponent {

  @Input({ required: true }) menu: MenuItem[] = [];

  private readonly authState = inject(AuthStateService);
  private readonly auth = inject(AuthService);

  dropdownOpen = false;

  get userName(): string {
    return this.authState.user?.name
      || this.authState.user?.email
      || 'User';
  }

  get role(): string | null {
    return this.authState.role;
  }

  get loggedIn(): boolean {
    return this.authState.isLoggedIn;
  }

  get visibleMenu(): MenuItem[] {
    if (!this.role) {return []};
    return this.menu.filter(
      item => !item.roles || item.roles.includes(this.role as any)
    );
  }

  toggleDropdown(): void {
    this.dropdownOpen = !this.dropdownOpen;
  }

  logout(): void {
    this.dropdownOpen = false;
    this.auth.logout();
    this.authState.clear();
    window.location.href = 'http://localhost:4200';
  }
}
