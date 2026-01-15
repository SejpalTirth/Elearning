import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CourseFacade } from '@frontend/core';

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

  private readonly courseApi = inject(CourseFacade);
  private readonly router = inject(Router);

  // Small guard to ensure we only check overflow after list renders
  private _needsOverflowCheck = false;

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses():void {
    this.courseApi.getAllCourses().subscribe({
      next: (res: any) => {
        // Keep your existing filtering if needed (published only etc.)
        this.courses = (res || []).filter((c: any) => c.isDeleted === false && c.isDraft === false);
        this.loading = false;

        // Mark for overflow checking on next tick after DOM paints
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

    // Loop through nodes and set corresponding course.isTruncated flag
    descNodes.forEach((p, idx) => {
      // Measure overflow: scrollHeight > clientHeight indicates hidden content
      const isOverflowing = p.scrollHeight > p.clientHeight + 1; // +1 small tolerance

      // Safety: ensure we don't go out of bounds
      if (this.courses[idx]) {
        this.courses[idx].isTruncated = isOverflowing;
      }
    });

    // Reset flag so further checks aren't required unless you reload items
    this._needsOverflowCheck = false;
  }
}