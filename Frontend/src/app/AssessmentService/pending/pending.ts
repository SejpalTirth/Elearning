import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ToastService } from '../../shared/toast.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pending',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pending.html',
  styleUrls: ['./pending.css']
})
export class Pending implements OnInit {

  userId: string = '';
  course: any = null;
  pendingModules: number[] = [];

  private courseGateway = "https://localhost:7249/api/GatewayCourse";
  private assessmentGateway = "https://localhost:7249/api/AssessmentGateway";

  constructor(
    private http: HttpClient,
    private router: Router,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    const token = localStorage.getItem("token");
    if (!token) return;

    const payload = JSON.parse(atob(token.split('.')[1]));
    this.userId = payload.sub;

    this.loadUnfinishedCourse();
  }

  loadUnfinishedCourse() {
    this.http.get(`${this.courseGateway}/unfinished/${this.userId}`).subscribe({
      next: (res: any) => {
        if (!res) {
          this.course = null;
          this.toastService.showSuccess("No pending tasks 🎉");
          return;
        }

        this.course = res;
        this.loadPendingModules(res.id);
      },
      error: () => {
        this.course = null;
        this.toastService.showError("Failed to load pending tasks.");
      }
    });
  }

  loadPendingModules(courseId: number) {
    this.http.get<number[]>(`${this.assessmentGateway}/unquizzed-modules/${courseId}`)
      .subscribe({
        next: (moduleIds) => {
          this.pendingModules = moduleIds || [];

          if (this.pendingModules.length > 0) {
            this.toastService.showError("Some modules still need quizzes.");
          }
        }
      });
  }

  continueQuiz() {
    if (!this.course) return;
    this.router.navigate(['/assessment/add-quiz', this.course.id]);
  }
}
