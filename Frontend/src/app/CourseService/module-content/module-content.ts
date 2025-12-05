import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CourseApiService } from '../services/course-api';
import { AssessmentApiService } from '../../AssessmentService/services/assessment-api';

@Component({
  selector: 'app-module-content',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './module-content.html',
  styleUrls: ['./module-content.css']
})
export class ModuleContentComponent implements OnInit {

  module: any = null;
  moduleId!: number;
  quizStatus: any = null;
  userId: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private api: CourseApiService,
    private assessmentApi: AssessmentApiService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.extractUserId();
    this.moduleId = Number(this.route.snapshot.paramMap.get('moduleId'));
    this.loadModule();
    this.checkQuizStatus();
  }

  extractUserId() {
    const token = localStorage.getItem('accessToken');
    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.userId = payload["sub"];
    } catch { }
  }

  loadModule() {
    this.api.getModuleById(this.moduleId).subscribe({
      next: (res) => this.module = res,
      error: (err) => console.error(err)
    });
  }

  checkQuizStatus() {
    if (!this.userId) return;

    this.assessmentApi.getQuizForModule(this.moduleId).subscribe({
      next: (res) => this.quizStatus = res,
      error: (err) => console.error(err)
    });
  }

  takeQuiz() {
    if (!this.quizStatus?.quizId) {
      alert("No quiz available.");
      return;
    }
    this.router.navigate([`/assessment/take/${this.moduleId}`]);
  }
}
