import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastService } from 'app/shared/toast.service';
import { CourseApiService } from '../../CourseService/services/course-api';
import { AssessmentApiService } from '../../AssessmentService/services/assessment-api';
import { SecureTokenService } from '../Security/secure-token.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home implements OnInit {

  userName: string | null = null;
  role: string | null = null;

  private readonly tokenService = inject(SecureTokenService);
  private readonly toastService = inject(ToastService);
  private readonly assessmentApi = inject(AssessmentApiService);
  private readonly courseApi = inject(CourseApiService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.loadUserInfo();

    if (this.role === 'Instructor') {
      this.checkForPendingTasks();
    }
  }

  // ------------------ Load User Info From Decrypted JWT ------------------
  loadUserInfo(): void {
    const payload = this.tokenService.getPayload();

    if (!payload) {
      this.router.navigate(['/']);
      return;
    }

    this.userName = payload['name'] || 'User';
    this.role = payload['role'] || null;
  }

  // ------------------ Check If Instructor Has Unfinished Course ------------------
  checkForPendingTasks(): void {
    const userId = this.tokenService.getUserId();
    if (!userId) { return; }

    this.courseApi.getInstructorUnfinishedCourses(userId).subscribe({
      next: (course: any): void => {
        if (!course || !course.id) { return; }

        const courseId = course.id;

        this.assessmentApi.getUnquizzedModules(courseId).subscribe({
          next: (modules: number[]): void => {
            if (modules && modules.length > 0) {
              this.toastService.showError(
                'You have pending course tasks — quizzes need to be completed.'
              );
            }
          },
          error: (err: any): void => {
            console.error('Failed to fetch unquizzed modules', err);
          }
        });
      },
      error: (err: any): void => {
        console.error('Failed to fetch unfinished courses', err);
      }
    });
  }

  goToCourses(): void {
    this.router.navigate(['/courses']);
  }
}
