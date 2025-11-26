import { Component, OnInit } from '@angular/core';
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
  userId = "";

  constructor(
    private route: ActivatedRoute,
    private courseApi: CourseApiService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));

    this.loadCourse(this.courseId);
    this.extractUserId();
    this.checkEnrollment();
  }

  extractUserId() {
    const token = localStorage.getItem('token');
    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.userId = payload["sub"];  // logged in user
    } catch {
      console.warn("Invalid token");
    }
  }

  loadCourse(id: number) {
    this.courseApi.getById(id).subscribe({
      next: (res) => this.course = res,
      error: (err) => console.error(err)
    });
  }

  checkEnrollment() {
    if (!this.userId) return;

    this.courseApi.getEnrolledCourses(this.userId).subscribe({
      next: (courses) => {
        this.isEnrolled = courses.some(c => c.id === this.courseId);
      },
      error: (err) => console.error(err)
    });
  }

  enrollOrContinue() {
    if (this.isEnrolled) {
      // Already enrolled → Continue Learning
      this.router.navigate([`/courses/${this.courseId}/modules`]);
      return;
    }

    // Not enrolled → Enroll now
    this.courseApi.enroll({
      courseId: this.courseId,
      userId: this.userId
    }).subscribe({
      next: () => {
        this.isEnrolled = true;
        this.router.navigate([`/courses/${this.courseId}/modules`]);
      },
      error: (err) => console.error(err)
    });
  }
}
