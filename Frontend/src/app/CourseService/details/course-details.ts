import { Component, inject, OnInit, Renderer2 } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CourseApiService } from '../services/course-api';
import { CommonModule } from '@angular/common';
import { SecureTokenService } from 'app/GatewayService/Security/secure-token.service';

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
  userId = '';

  isLoading = false;
  btnLoading = false;
  
  private readonly route = inject(ActivatedRoute);
  private readonly courseApi = inject(CourseApiService);
  private readonly router = inject(Router);
  private readonly renderer = inject(Renderer2);
  private readonly tokenService = inject(SecureTokenService);

  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));

    this.loadCourse(this.courseId);
    this.extractUserId();
    this.checkEnrollment();
  }

  extractUserId():void {
    const userId = this.tokenService.getUserId();

    this.userId = userId ?? '';
  }



  loadCourse(id: number): void {
    this.courseApi.getById(id).subscribe({
      next: (res) => this.course = res
    });
  }

  checkEnrollment(): void {
    if (!this.userId) {return;}

    this.courseApi.getEnrolledCourses(this.userId).subscribe({
      next: (courses) => {
        this.isEnrolled = courses.some((c: { id: number; }) => c.id === this.courseId);
      }
    });
  }
  disableNavbarClicks(): void {
    const nav = document.querySelector('nav');
    if (nav) {this.renderer.setStyle(nav, 'pointer-events', 'none');}
  }
  enableNavbarClicks():void {
    const nav = document.querySelector('nav');
    if (nav) {this.renderer.setStyle(nav, 'pointer-events', 'auto');}
  }

  enrollOrContinue():void  {
    if (this.isEnrolled) {
      this.router.navigate([`/courses/${this.courseId}/modules`]);
      return;
    }

    // Start loading
    this.btnLoading = true;
    this.isLoading = true;
    this.disableNavbarClicks();

    this.courseApi.enroll({
      courseId: this.courseId,
      userId: this.userId
    }).subscribe({
      next: () => {
        this.isEnrolled = true;


        setTimeout(() => {
          this.enableNavbarClicks();
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
