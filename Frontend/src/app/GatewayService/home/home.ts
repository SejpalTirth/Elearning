import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { ToastService } from 'app/shared/toast.service';

@Component({
  selector: 'app-home',
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home implements OnInit {

  userName: string | null = null;
  role: string | null = null;
  userId: string | null = null;
  unfinishedCourse: any = null;

  private gatewayUrl = 'https://localhost:7249/api/GatewayCourse';

  constructor(
    private router: Router,
    private http: HttpClient,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadUserInfo();

    // Load unfinished course only for instructors
    if (this.role === 'Instructor') {
      this.loadUnfinishedCourse();
    }
  }

  // ------------------ LOAD USER INFO FROM JWT ------------------
  loadUserInfo() {
    const token = localStorage.getItem('token');

    if (!token) {
      this.router.navigate(['/']);
      return;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));

      this.userName = payload['name'] || 'User';
      this.role = payload['role'] || null;
      this.userId = payload['sub'] || null;   // <-- FIX ADDED HERE

    } catch (err) {
      console.error("Failed to decode token", err);
      this.router.navigate(['/']);
    }
  }

  // ------------------ LOAD UNFINISHED COURSE ------------------
  loadUnfinishedCourse() {
    if (!this.userId) return; // safety check

    this.http.get(`${this.gatewayUrl}/unfinished/${this.userId}`).subscribe({
      next: (res: any) => {
        if (res && res.id) {
          this.toastService.showError(
            "You have an unfinished course. Continue quiz creation!"
          );
        }
      },
      error: () => {}
    });
  }

  continueCourse() {
    if (!this.unfinishedCourse) return;

    const courseId = this.unfinishedCourse.id;
    this.router.navigate(['/assessment/add-quiz', courseId]);
  }

  goToCourses() {
    this.router.navigate(['/courses']);
  }
}
