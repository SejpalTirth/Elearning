import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { ProgressService } from '../../CourseService/services/progress.service';
import { SecureTokenService } from 'app/GatewayService/Security/secure-token.service';

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

  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(CourseApiService);
  private readonly progressService = inject(ProgressService);
  private readonly router = inject(Router);
  private readonly tokenService  = inject(SecureTokenService);

  ngOnInit(): void {
    this.extractUserId();
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadData();
  }

  extractUserId():void {
    this.userId = this.tokenService.getUserId();
  }

  loadData(): void {

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

  openModule(moduleId: number): void {
    this.router.navigate([`/courses/module/${moduleId}`]);
  }
}
