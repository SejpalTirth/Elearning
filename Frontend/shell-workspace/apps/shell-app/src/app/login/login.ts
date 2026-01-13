import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LoadingService } from '@frontend/ui';
import { AuthService } from '@frontend/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule,RouterModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent implements OnInit {

  email = '';
  password = '';
  loginMessage: string | null = null;
  error: string | null = null;

  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly loader = inject(LoadingService);

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['success'] === 'true') {
        this.loginMessage = 'Successfully Logged In!';
      }
    });
  }

  loginWithGoogle(): void {
    this.loader.show();
    this.auth.loginWithGoogle();
  }

  loginWithMicrosoft(): void {
    this.loader.show();
    this.auth.loginWithMicrosoft();
  }

  loginWithEmail(): void {
  this.error = null;

  if (!this.email.trim() || !this.password) {
    this.error = 'Please enter both email and password';
    return;
  }

  this.loader.show();

  this.auth.localLogin(this.email, this.password).subscribe({
    next: (response: any) => {
      // Only redirect if login actually succeeded
      if (response?.success) {
        window.location.href = 'http://localhost:4200/gateway/auth/callback';
        this.loader.hide();
      } else {
        this.error = response?.message || 'Login failed';
        this.loader.hide();
      }
    },
    error: (err) => {
      this.error = err?.error?.message || 'Login failed';
      this.loader.hide();
    },
    complete: () => {
      this.loader.hide();
    }
  });
}


}
