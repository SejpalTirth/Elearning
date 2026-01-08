import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AssessmentFacade } from '@frontend/core';
import { AssessmentGatewayService } from '@frontend/api';

@Component({
  selector: 'app-quiz-result',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './quiz-result.html',
  styleUrls: ['./quiz-result.css']
})
export class QuizResultComponent implements OnInit {

  submissionId!: string;
  moduleId!: number;
  result: any;
  loading = true;

  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(AssessmentFacade);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.submissionId = this.route.snapshot.paramMap.get('submissionId') || '';

    this.moduleId = Number(this.route.snapshot.queryParamMap.get('moduleId'));

    this.loadResult();
  }

  loadResult(): void {
    this.api.getQuizResult({submissionId: this.submissionId}).subscribe({
      next: (res) => {
        this.result = res;
        this.loading = false;
      },
      error: err => console.error(err)
    });
  }

  backToModules(): void {
    if (this.moduleId) {
      this.router.navigate([`/courses/module/${this.moduleId}`]);
    } else {
      this.router.navigate(['/courses']);
    }
  }
}