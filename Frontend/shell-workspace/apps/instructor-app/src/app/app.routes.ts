import { Routes } from '@angular/router';
import { 
  AddCourseComponent,
  CourseDetailsComponent,
  EditCourseComponent,
  InstructorCourseList,
  ListCoursesComponent,
  ModuleContentComponent,
  ModulesListComponent,
  MyLearningComponent
}
from '@frontend/features/courses'
import {
  AddQuizComponent,
  Pending,
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
      {path: 'add-course',component: AddCourseComponent},
      {path: 'my-courses',component: InstructorCourseList},
      {path: 'courses/edit/:id',component: EditCourseComponent},
      {path: 'courses/module/:moduleId',component: ModuleContentComponent},
      {path: 'quiz/:moduleId',component: TakeQuizComponent},
      {path: 'assessment/result/:submissionId',component: QuizResultComponent},
      {path: 'pending',component: Pending},
      {path: 'assessment/add-quiz/:courseId',component: AddQuizComponent},
    ]
    },
    {
    path: '**',
    redirectTo: 'access-denied'
  }
];
