import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { ProgressService } from '../services/progress.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-my-learning',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-learning.html',
  styleUrls: ['./my-learning.css']
})
export class MyLearningComponent implements OnInit {

  courses: any[] = [];
  progress: any[] = [];
  loading = true;
  userId: string | null = null;

  constructor(
    private api: CourseApiService,
    private progressService: ProgressService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.extractUserId();
    this.loadMyCourses();
  }

  extractUserId() {

    const token = localStorage.getItem('accessToken');

    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.userId = payload.sub;
    } catch {}
  }

  loadMyCourses() {
  if (!this.userId) {
    this.loading = false;
    return;
  }

  this.progressService.getUserProgress(this.userId).subscribe((p: any) => {
    this.progress = p;

    this.api.getEnrolledCourses(this.userId!).subscribe({
      next: (res) => {
        this.courses = res.map(course => {
          
          const courseProgressItems = this.progress.filter(pr => pr.courseId === course.id);

          const completedModules = courseProgressItems.length;
          const totalModules = course.modules?.length || 0;

          const percent = totalModules > 0
          ? Math.min(100, Math.round((completedModules / totalModules) * 100))
          : 0;


          return { 
            ...course, 
            progressPercent: percent,
            completedModules,
            totalModules
          };
        });

        this.loading = false;
      },
      error: () => (this.loading = false)
    });

  });
}



  getProgress(courseId: number) {
    const entries = this.progress.filter(p => p.courseId === courseId);
    if (!entries.length) return 0;

    return Math.round(entries.reduce((sum, p) => sum + p.progressPercent, 0) / entries.length);
  }

  continueLearning(courseId: number) {
    localStorage.setItem("currentCourseId", courseId.toString());
    this.router.navigate([`/courses/${courseId}/modules`]);
  }
}
