import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  templateUrl: './auth-callback.html',
  styleUrl: './auth-callback.css'
})
export class AuthCallback implements OnInit {

  constructor(private router: Router) {}

  ngOnInit() {
    const params = new URLSearchParams(window.location.search);

    const isNewUser = params.get('isNewUser');
    const userId = params.get('userId');

    const accessToken = params.get('token');
    const refreshToken = params.get('refresh')?.replace(/ /g, '+');

    // NEW USER → redirect
    if (isNewUser === 'true' && userId) {
      this.router.navigate(['/complete-profile'], {
        queryParams: { userId }
      });
      return;
    }

    // EXISTING USER → store tokens with correct keys
    if (accessToken && refreshToken) {

      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);

      setTimeout(() => this.router.navigate(['/home']), 300);
      return;
    }

    // fallback
    this.router.navigate(['/login']);
  }
}
