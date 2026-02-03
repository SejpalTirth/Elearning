import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToastService } from '@frontend/ui';
import { GatewayUsersService } from '@frontend/api';

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
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly userapi = inject(GatewayUsersService);

  ngOnInit(): void {
    this.userId = this.route.snapshot.queryParamMap.get('userId');

    if (!this.userId) {
      this.toast.showError('User ID missing. Please login again.');
      this.router.navigate(['/login']);
      return;
    }
  }

  saveProfile(): void {
  if (!this.name.trim()) {
    this.toast.showError('Please enter your name');
    return;
  }

  const role = ROLE_MAP[this.roleId];

  if (!role) {
    this.toast.showError('Invalid role selected');
    return;
  }

  this.loading = true;

  const payload = {
    userId: this.userId!,
    name: this.name,
    role
  };

  this.userapi.postApiUsersCompleteProfile(payload).subscribe({
    next: () => {
      this.toast.showInfo('Profile completed successfully! Please login again.')
      this.router.navigate(['/']);
    },
    error: (err) => {
      this.loading = false;
      this.toast.showError('Something went wrong.');
    }
  });
}

}
const ROLE_MAP: Record<number, string> = {
  1: 'Admin',
  2: 'Instructor',
  3: 'Student'
};