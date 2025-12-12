import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { environment } from 'Environment/environment';
import { AuthService } from 'app/GatewayService/Auth/auth.service';
import { SecureTokenService } from 'app/GatewayService/Security/secure-token.service';

@Component({
  selector: 'app-shared-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class SharedHeaderComponent implements OnInit {

  userName: string | null = null;
  role: string | null = null;
  loggedIn = false;

  dropdownOpen = false;

  private readonly url = `${environment.baseapiurl}/Gatewayauth`;

  private readonly auth = inject(AuthService);
  private readonly tokenService = inject(SecureTokenService);


  ngOnInit(): void {
    const payload = this.tokenService.getPayload();

    if (payload) {
      this.userName = payload.name || payload.email || 'User';
      this.role = payload.role || null;
      this.loggedIn = true;
    } else {
      this.loggedIn = false;
    }

  }

  toggleDropdown(): void {
    this.dropdownOpen = !this.dropdownOpen;
  }

  logout(): void {
    this.auth.logout();
  }
}
