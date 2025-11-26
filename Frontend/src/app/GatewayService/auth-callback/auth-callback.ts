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

    // NEW USER CHECK
    const isNewUser = params.get('isNewUser');
    const userId = params.get('userId');

    // EXISTING USER TOKENS
    const token = params.get('token');
    const refresh = params.get('refresh');

    // If new user → redirect to complete-profile
    if (isNewUser === 'true' && userId) {
      this.router.navigate(
        ['/complete-profile'],
        { queryParams: { userId } }
      );
      return;
    }

    // If tokens exist → store and redirect to home
    if (token && refresh) {
      localStorage.setItem('token', token);
      localStorage.setItem('refresh', refresh);

      setTimeout(() => {
        this.router.navigate(['/home']);
      }, 600);
      
      return;
    }

    // Fallback
    this.router.navigate(['/login']);
  }
}
