import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'app/GatewayService/Auth/auth.service';
import { AuthStateService } from 'app/GatewayService/Auth/auth-state.service';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  templateUrl: './auth-callback.html',
  styleUrls: ['./auth-callback.css']
})
export class AuthCallback implements OnInit {

  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly authState = inject(AuthStateService);

  ngOnInit(): void {

    const params = new URLSearchParams(window.location.search);

    const isNewUser = params.get('isNewUser');
    const userId = params.get('userId');

    const accessToken = params.get('token');
    const refreshToken = params.get('refresh')?.replace(/ /g, '+');

    // -------------------------------
    // NEW USER → Go to Complete Profile
    // -------------------------------
    if (isNewUser === 'true' && userId) {
      this.router.navigate(['/complete-profile'], {
        queryParams: { userId },
        replaceUrl : true
      });
      return;
    }

    // -------------------------------
    // EXISTING USER → Store tokens
    // -------------------------------
   if (accessToken && refreshToken) {
    this.auth.storeTokens({
      accessToken,
      refreshToken
    });

    this.authState.onLoginSuccess();

    this.router.navigate(['/home'], {
      replaceUrl: true
    });

    return;
  }



    // -------------------------------
    // FALLBACK
    // -------------------------------
    this.router.navigate(['/login']);
  }
}
