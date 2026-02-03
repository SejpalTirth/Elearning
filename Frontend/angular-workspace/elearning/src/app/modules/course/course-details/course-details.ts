import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { GatewayCourseService } from 'api';

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
  private readonly router = inject(Router);
  private readonly courseapi = inject(GatewayCourseService);

  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCourse();
    this.checkEnrollment();
  }

  // -----------------------------
  // DATA
  // -----------------------------
  loadCourse(): void {
    this.courseapi.postApiCourseById({courseId: this.courseId}).subscribe({
      next: res => this.course = res
    });
  }

  checkEnrollment(): void {
    this.courseapi.postApiCourseEnrolled().subscribe({
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

    this.courseapi.postApiCourseEnroll({courseId: this.courseId}).subscribe({
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