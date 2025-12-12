import { Component, inject, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { AssessmentApiService } from '../../AssessmentService/services/assessment-api';
import { SecureTokenService } from 'app/GatewayService/Security/secure-token.service';
import { LoadingOverlay } from '../../shared/loading/loading-overlay';

@Component({
  selector: 'app-module-content',
  standalone: true,
  imports: [CommonModule, LoadingOverlay],
  templateUrl: './module-content.html',
  styleUrls: ['./module-content.css']
})
export class ModuleContentComponent implements OnInit, AfterViewInit {

  module: any = null;
  moduleId!: number;
  quizStatus: any = null;
  userId: string | null = null;

  isLoaded = false;

  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(CourseApiService);
  private readonly assessmentApi = inject(AssessmentApiService);
  private readonly router = inject(Router);
  private readonly tokenService = inject(SecureTokenService);

  @ViewChild('loader') loader!: LoadingOverlay;

  ngOnInit(): void {
    // Initialize IDs here — ViewChild is NOT available yet
    this.userId = this.tokenService.getUserId();
    this.moduleId = Number(this.route.snapshot.paramMap.get('moduleId'));
  }

  ngAfterViewInit(): void {
  setTimeout(() => {
    this.loadFullData();
  });
}
  loadFullData(): void {
    this.loader.show();

    Promise.all([
      this.loadModule(),
      this.checkQuizStatus()
    ]).finally(() => {
      this.isLoaded = true;
      this.loader.hide();
    });
  }

  loadModule(): Promise<void> {
    return new Promise((resolve) => {
      this.api.getModuleById(this.moduleId).subscribe({
        next: res => {
          this.module = res;
          resolve();
        },
        error: () => resolve()
      });
    });
  }

  checkQuizStatus(): Promise<void> {
    return new Promise<void>((resolve) => {
      if (!this.userId) {
        resolve();
        return; // Ensure consistent return
      }

      this.assessmentApi.getQuizForModule(this.moduleId).subscribe({
        next: (res) => {
          this.quizStatus = res;
          resolve();
        },
        error: () => resolve()
      });
    });
  }


  takeQuiz(): void {
    if (!this.quizStatus?.quizId) {
      // eslint-disable-next-line no-alert
      alert('No quiz available.');
      return;
    }
    this.router.navigate([`/assessment/take/${this.moduleId}`]);
  }
}
