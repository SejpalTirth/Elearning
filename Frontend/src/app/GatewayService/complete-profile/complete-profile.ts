import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserApiService } from '../../UserService/services/user-api';  // Adjust path if needed

@Component({
  selector: 'app-complete-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './complete-profile.html',
  styleUrls: ['./complete-profile.css'],
})
export class CompleteProfileComponent implements OnInit {

  userId: string | null = null;
  name = '';
  roleId = 3;
  loading = false;

  private readonly route = inject(ActivatedRoute);
  private readonly userApi = inject(UserApiService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.userId = this.route.snapshot.queryParamMap.get('userId');

    if (!this.userId) {
      // eslint-disable-next-line no-alert
      alert('User ID missing. Please login again.');
      this.router.navigate(['/login']);
      return;
    }
  }

  saveProfile(): void {
    if (!this.name.trim()) {
      // eslint-disable-next-line no-alert
      alert('Please enter your name.');
      return;
    }

    this.loading = true;

    const payload = {
      userId: this.userId!,
      name: this.name,
      roleId: this.roleId
    };

    this.userApi.completeProfile(payload).subscribe({
      next: () => {
        // eslint-disable-next-line no-alert
        alert('Profile completed successfully! Please login again.');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading = false;
        // eslint-disable-next-line no-alert
        alert(err.error?.message || 'Something went wrong.');
      }
    });
  }
}
