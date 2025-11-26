import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { ProgressService } from '../../CourseService/services/progress.service';

@Component({
  selector: 'app-modules-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modules-list.html',
  styleUrls: ['./modules-list.css']
})
export class ModulesListComponent implements OnInit {

  modules: any[] = [];
  courseId!: number;
  loading = true;
  userId: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private api: CourseApiService,
    private progressService: ProgressService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.extractUserId();
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadData();
  }

  extractUserId() {
    const token = localStorage.getItem('token');
    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.userId = payload["sub"];
    } catch {}
  }

  loadData() {

    if (!this.userId) {
      this.loading = false;
      return;
    }

    // Fetch progress first
    this.progressService.getUserProgress(this.userId).subscribe((progress: any[]) => {

      // Then load modules
      this.api.getModules(this.courseId).subscribe({
        next: (res: any[]) => {

          this.modules = res.map(m => {
            const match = progress.find(p => p.moduleId === m.id);
            return {
              ...m,
              progressPercent: match ? match.progressPercent : 0,
              isCompleted: match ? match.progressPercent === 100 : false
            };
          });

          this.loading = false;
        },
        error: () => this.loading = false
      });

    });
  }

  openModule(moduleId: number) {
    this.router.navigate([`/courses/module/${moduleId}`]);
  }
}
