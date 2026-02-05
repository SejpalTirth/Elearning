import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LoadingService } from '../../../common-modules/ui/loading/loading-service';
import { AuthService } from '../../../service/auth-service';
import { LOCATION_TOKEN } from '../../../common-modules/tokens/location.token';

@Component({
  selector: 'app-sign-up',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sign-up.html',
  styleUrls: ['./sign-up.css']
})
export class SignUpComponent {
  email = '';
  password = '';
  confirmPassword = '';

  showPassword = false;
  showConfirmPassword = false;

  error: string | null = null;

  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly loader = inject(LoadingService);
  private readonly location = inject(LOCATION_TOKEN);

  private readonly passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$/;

  register(): void {
    this.error = null;

    if (this.password !== this.confirmPassword) {
      this.error = 'Passwords do not match';
      return;
    }

    if (!this.passwordRegex.test(this.password)) {
      this.error = 'Password must be at least 8 characters and include uppercase, lowercase, number, and special character';
      return;
    }

    this.loader.show();

    this.auth.localRegister(this.email, this.password).subscribe({
      next: (res: any) => {
        this.loader.hide();
        if (res?.isNewUser && res.userId) {
          this.location.href = `http://localhost:4200/gateway/auth/callback?isNewUser=true&userId=${res.userId}`;
        }
      },
      error: err => {
        this.loader.hide();
        this.error = err?.error?.message || 'Registration failed';
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/']);
  }
}