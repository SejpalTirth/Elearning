import { Component, OnInit, Renderer2 } from '@angular/core';
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

  isLoading = false;      // overlay loader + block UI
  btnLoading = false;     // loader inside button

  constructor(
    private route: ActivatedRoute,
    private courseApi: CourseApiService,
    private router: Router,
    private renderer: Renderer2     // NEW: disable nav click
  ) {}

  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));

    this.loadCourse(this.courseId);
    this.extractUserId();
    this.checkEnrollment();
  }

  extractUserId() {
    const token = localStorage.getItem('accessToken');
    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.userId = payload["sub"];
    } catch {}
  }

  loadCourse(id: number) {
    this.courseApi.getById(id).subscribe({
      next: (res) => this.course = res
    });
  }

  checkEnrollment() {
    if (!this.userId) return;

    this.courseApi.getEnrolledCourses(this.userId).subscribe({
      next: (courses) => {
        this.isEnrolled = courses.some(c => c.id === this.courseId);
      }
    });
  }

  // ❗ Disable navbar clicks
  disableNavbarClicks() {
    const nav = document.querySelector("nav");
    if (nav) this.renderer.setStyle(nav, "pointer-events", "none");
  }

  // ❗ Enable navbar clicks
  enableNavbarClicks() {
    const nav = document.querySelector("nav");
    if (nav) this.renderer.setStyle(nav, "pointer-events", "auto");
  }

  enrollOrContinue() {
    if (this.isEnrolled) {
      this.router.navigate([`/courses/${this.courseId}/modules`]);
      return;
    }

    // Start loading
    this.btnLoading = true;
    this.isLoading = true;
    this.disableNavbarClicks();     //  block navbar clicks

    this.courseApi.enroll({
      courseId: this.courseId,
      userId: this.userId
    }).subscribe({
      next: () => {
        this.isEnrolled = true;

        // Small delay for smooth UX
        setTimeout(() => {
          this.enableNavbarClicks();     // ✔ re-enable navbar
          this.router.navigate([`/courses/${this.courseId}/modules`]);
        }, 800);
      },
      error: () => {
        this.enableNavbarClicks();
        this.btnLoading = false;
        this.isLoading = false;
      }
    });
  }
}
