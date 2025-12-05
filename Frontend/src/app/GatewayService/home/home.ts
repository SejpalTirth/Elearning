import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { ToastService } from 'app/shared/toast.service';

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

  private gatewayCourseUrl = 'https://localhost:7249/api/GatewayCourse';
  private gatewayAssessmentUrl = 'https://localhost:7249/api/AssessmentGateway';

  constructor(
    private router: Router,
    private http: HttpClient,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadUserInfo();

    // Only instructors should see pending task reminders
    if (this.role === 'Instructor') {
      this.checkForPendingTasks();
    }
  }

  // ------------------ Load User Info From JWT ------------------
  loadUserInfo() {
    const token = localStorage.getItem('accessToken');

    if (!token) {
      this.router.navigate(['/']);
      return;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));

      this.userName = payload['name'] || 'User';
      this.role = payload['role'] || null;

    } catch (err) {
      console.error('JWT Decode Failed:', err);
      this.router.navigate(['/']);
    }
  }

  // ------------------ Check If Instructor Has Unfinished Course ------------------
  checkForPendingTasks() {
    const token = localStorage.getItem('accessToken');
    if (!token) return;

    const payload = JSON.parse(atob(token.split('.')[1]));
    const userId = payload.sub;

    // STEP 1 — Fetch unfinished course
    this.http.get<any>(`${this.gatewayCourseUrl}/unfinished/${userId}`).subscribe({
      next: (course) => {
        if (!course || !course.id) {
          return; // no unfinished course
        }

        const courseId = course.id;

        // STEP 2 — Check quiz status
        this.http.get<number[]>(
          `${this.gatewayAssessmentUrl}/unquizzed-modules/${courseId}`
        ).subscribe({
          next: (modules) => {
            if (modules && modules.length > 0) {
              // Modules missing quizzes → SHOW TOAST
              this.toastService.showError(
                "You have pending course tasks — quizzes need to be completed."
              );
            }
          },
          error: () => {}
        });
      },
      error: () => {}
    });
  }

  // ------------------ Navigation ------------------
  goToCourses() {
    this.router.navigate(['/courses']);
  }
}
