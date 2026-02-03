import { Routes } from "@angular/router";

export const assessmentRoutes : Routes = [
    {
        path : 'pending',
        loadComponent: () =>
            import('./pending/pending').then(m => m.Pending)
    },
    {
        path : 'add-quiz/:courseId',
        loadComponent: () =>
            import('./add-quiz/add-quiz').then(m => m.AddQuizComponent)
    },
    {
        path : 'result/:submissionId',
        loadComponent: () =>
            import('./quiz-result/quiz-result').then(m => m.QuizResultComponent)
    },
    {
        path : 'quiz/:moduleId',
        loadComponent: () =>
            import('./take-quiz/take-quiz').then(m => m.TakeQuizComponent)
    }
]