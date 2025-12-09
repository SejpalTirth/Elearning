import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AssessmentApiService } from '../services/assessment-api';
import { CommonModule } from '@angular/common';

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

  constructor(
    private route: ActivatedRoute,
    private api: AssessmentApiService,
    private router: Router
  ) {}

  ngOnInit() {
    this.submissionId = this.route.snapshot.paramMap.get('submissionId') || '';
    
    this.moduleId = Number(this.route.snapshot.queryParamMap.get('moduleId'));

    this.loadResult();
  }

  loadResult() {
    this.api.getResult(this.submissionId).subscribe({
      next: (res) => {
        this.result = res;
        this.loading = false;
      },
      error: err => console.error(err)
    });
  }

  backToModules() {
    if (this.moduleId) {
      this.router.navigate([`/courses/module/${this.moduleId}`]);
    } else {
      this.router.navigate(['/courses']); // fallback
    }
  }
}
