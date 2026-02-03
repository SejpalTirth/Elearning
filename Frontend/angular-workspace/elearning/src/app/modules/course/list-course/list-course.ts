import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { GatewayCourseService } from 'api';

@Component({
  selector: 'app-list-courses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './list-course.html',
  styleUrls: ['./list-course.css']
})
export class ListCoursesComponent implements OnInit {

  courses: any[] = [];
  loading = true;

  private readonly router = inject(Router);
  private readonly courseapi = inject(GatewayCourseService)

  private _needsOverflowCheck = false;

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses():void {
    this.courseapi.postApiCourseAll().subscribe({
      next: (res: any) => {
        this.courses = (res || []).filter((c: any) => c.isDeleted === false && c.isDraft === false);
        this.loading = false;

        this._needsOverflowCheck = true;
        setTimeout(() => this.checkAllOverflows(), 0);
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  openCourse(id: number):void {
    this.router.navigate(['/courses', id]);
  }

  // --------------------------
  // Overflow detection
  // --------------------------
  private checkAllOverflows():void {
    const descNodes = Array.from(document.querySelectorAll<HTMLParagraphElement>('.course-card .description'));

    descNodes.forEach((p, idx) => {
      const isOverflowing = p.scrollHeight > p.clientHeight + 1;

      if (this.courses[idx]) {
        this.courses[idx].isTruncated = isOverflowing;
      }
    });

    this._needsOverflowCheck = false;
  }
}