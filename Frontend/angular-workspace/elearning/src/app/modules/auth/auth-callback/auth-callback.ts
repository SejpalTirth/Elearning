import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { filter, take } from 'rxjs/operators';
import { AuthStateService } from '../../../service/auth-state-service';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  templateUrl: './auth-callback.html',
  styleUrls: ['./auth-callback.css']
})
export class AuthCallback implements OnInit {

  private readonly router = inject(Router);
  private readonly authState = inject(AuthStateService);

  ngOnInit(): void {
    const params = new URLSearchParams(window.location.search);

    const isNewUser = params.get('isNewUser');
    const userId = params.get('userId');

    // ----------------------------------
    // NEW USER → complete profile
    // ----------------------------------
    if (isNewUser === 'true' && userId) {
      this.router.navigate(['/complete-profile'], {
        queryParams: { userId },
        replaceUrl: true
      });
      return;
    }

    // ----------------------------------
    // EXISTING USER
    // ----------------------------------
    // Trigger backend session validation
    this.authState.initialize();

    this.authState.user$
      .pipe(
        filter(user => !!user),
        take(1)
      )
      .subscribe({
        next: user => {
          this.router.navigate(['/home'])
        },
        error: () => {
          this.router.navigate(['/']);
        }
      });

    // Safety fallback (cookie missing / expired)
    setTimeout(() => {
      if (!this.authState.user) {
        this.router.navigate(['/']);
      }
    }, 4000);
  }
}