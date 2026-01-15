import { Routes } from '@angular/router';
import { 
  CourseDetailsComponent,
  ListCoursesComponent,
  ModuleContentComponent,
  ModulesListComponent,
  MyLearningComponent
 } 
 from '@frontend/features/courses'
 import{
   QuizResultComponent,
  TakeQuizComponent
 }
 from '@frontend/features/assessment'
import { DenialPage } from '@frontend/ui';
import { DenialLockGuard } from './guards/denial-lock.guard';
import { AppBoundaryGuard } from './guards/app-boundary.guard';

export const routes: Routes = [

    {
      path: 'access-denied',
      component: DenialPage,
      data : {layout : 'denied'},
      canDeactivate: [DenialLockGuard]
    },

    {
      path: '',
      canActivate: [AppBoundaryGuard],
      children: [

      {
        path: '',
        loadChildren: () =>
          import('@frontend/features/home').then(m => m.HOME_ROUTES)
      },

      {path: 'courses',component: ListCoursesComponent},
      {path: 'my-learning',component: MyLearningComponent},
      {path: 'courses/:id',component: CourseDetailsComponent},
      {path: 'courses/:id/modules',component: ModulesListComponent},
      {path: 'courses/module/:moduleId',component: ModuleContentComponent},
      {path: 'quiz/:moduleId',component: TakeQuizComponent},
      {path: 'assessment/result/:submissionId',component: QuizResultComponent},
    ]
    },
    {
    path: '**',
    redirectTo: 'access-denied'
  }
];


