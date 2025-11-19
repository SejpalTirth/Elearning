import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-auth-callback',
  templateUrl: './auth-callback.html',
  styleUrls: ['./auth-callback.css'],
})
export class AuthCallback implements OnInit {

  message = "Processing login...";

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      const refresh = params['refresh'];

      if (token && refresh) {
        // Save tokens
        localStorage.setItem('access_token', token);
        localStorage.setItem('refresh_token', refresh);

        this.message = "Successfully logged in! Redirecting...";

        // Redirect after delay
        setTimeout(() => {
          this.router.navigate(['/dashboard']);
        }, 1500);

      } else {
        this.message = "Login failed. Missing authentication tokens.";
      }
    });
  }
}
