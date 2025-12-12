import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-completed',
  imports: [],
  templateUrl: './completed.html',
  styleUrl: './completed.css',
})
export class CompletedComponent implements OnInit {
  quizTitle = '';
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.quizTitle = history.state.quizTitle ?? 'this module';
  }

  goBack(): void {
    this.router.navigate(['/my-learning']);
  }
}

