import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CourseApiService } from '../services/course-api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-course-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './course-details.html',
  styleUrls: ['./course-details.css']
})
export class CourseDetailsComponent implements OnInit {

  course: any = null;
  courseId!: number;
  isEnrolled = false;

  isLoading = false;
  btnLoading = false;

  private readonly route = inject(ActivatedRoute);
  private readonly courseApi = inject(CourseApiService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCourse();
    this.checkEnrollment();
  }

  // -----------------------------
  // DATA
  // -----------------------------
  loadCourse(): void {
    this.courseApi.getById(this.courseId).subscribe({
      next: res => this.course = res
    });
  }

  checkEnrollment(): void {
    this.courseApi.getEnrolledCourses().subscribe({
      next: courses => {
        this.isEnrolled = courses.some(
          (c: { id: number }) => c.id === this.courseId
        );
      }
    });
  }

  // -----------------------------
  // ACTION
  // -----------------------------
  enrollOrContinue(): void {

    // Already enrolled → just navigate
    if (this.isEnrolled) {
      this.router.navigate(['/courses', this.courseId, 'modules']);
      return;
    }

    // Start loader
    this.isLoading = true;
    this.btnLoading = true;

    this.courseApi.enroll(this.courseId).subscribe({
      next: () => {
        this.isEnrolled = true;

        // Navigate FIRST, cleanup AFTER
        this.router.navigate(['/courses', this.courseId, 'modules'])
          .then(() => {
            this.isLoading = false;
            this.btnLoading = false;
          });
      },
      error: err => {

        // Already enrolled edge-case
        if (err.status === 400) {
          this.router.navigate(['/courses', this.courseId, 'modules'])
            .then(() => {
              this.isLoading = false;
              this.btnLoading = false;
            });
          return;
        }

        // Real error
        this.isLoading = false;
        this.btnLoading = false;
      }
    });
  }
}
