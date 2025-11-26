import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-completed',
  imports: [],
  templateUrl: './completed.html',
  styleUrl: './completed.css',
})
export class CompletedComponent implements OnInit {
  quizTitle: string = '';

  constructor(private router: Router) {}

  ngOnInit() {
    this.quizTitle = history.state.quizTitle ?? "this module";
  }

  goBack() {
    this.router.navigate(['/my-learning']);
  }
}

