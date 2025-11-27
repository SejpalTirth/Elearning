import { Routes } from '@angular/router';

export const ASSESSMENT_ROUTES: Routes = [
  {
    path: 'take/:moduleId',
    loadComponent: () =>
      import('./take-quiz/take-quiz').then(m => m.TakeQuizComponent)
  },
  {
    path: 'result/:submissionId',
    loadComponent: () =>
      import('./quiz-result/quiz-result').then(m => m.QuizResultComponent)
  },
  {
    path: 'add-quiz/:courseId',
    loadComponent: () =>
      import('./add-quiz/add-quiz.component').then(m => m.AddQuizComponent)
  }
];
