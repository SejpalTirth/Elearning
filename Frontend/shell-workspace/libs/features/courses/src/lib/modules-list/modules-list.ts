import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthStateService } from '@frontend/auth';
import { combineLatest, Subscription } from 'rxjs';
import { GatewayCourseService, ProgressGatewayService } from '@frontend/api';

@Component({
  selector: 'app-modules-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modules-list.html',
  styleUrls: ['./modules-list.css']
})
export class ModulesListComponent implements OnInit, OnDestroy {

  modules: any[] = [];
  courseId!: number;
  loading = true;

  private readonly route = inject(ActivatedRoute);
  private readonly progressService = inject(ProgressGatewayService);
  private readonly authState = inject(AuthStateService);
  private readonly router = inject(Router);
  private readonly courseapi = inject(GatewayCourseService);

  private sub?: Subscription;

  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));

    this.sub = this.authState.user$.subscribe(user => {
      if (!user) {
        this.modules = [];
        this.loading = false;
        return;
      }

      this.progressService.postApiProgressUser().subscribe();

      this.bindModulesWithProgress();
      
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  private bindModulesWithProgress(): void {
  this.loading = true;

  this.sub = combineLatest([
    this.courseapi.postApiCourseModules({ courseId: this.courseId }),
    this.progressService.postApiProgressUser()
  ]).subscribe({
    next: ([modules, progress]) => {
      this.modules = modules.map(m => {
        const match = progress.find(
          p => p.courseId === this.courseId && p.moduleId === m.id
        );

        return {
          ...m,
          progressPercent: match?.progressPercent ?? 0,
          isCompleted: match?.isCompleted === true
        };
      });

      this.loading = false;
    },
    error: () => (this.loading = false)
  });
}



  openModule(moduleId: number): void {
    this.router.navigate([`/courses/module/${moduleId}`]);
  }
}