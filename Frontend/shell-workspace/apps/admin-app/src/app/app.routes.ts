import { Routes } from '@angular/router';
import { AppBoundaryGuard } from './guards/app-boundary.guard';
import { DenialLockGuard } from './guards/denial-lock.guard';

import {
  AdminManageCoursesComponent,
  CourseDetailsComponent,
  EditCourseComponent,
  ListCoursesComponent,
  ModuleContentComponent,
  ModulesListComponent,
  MyLearningComponent
} from '@frontend/features/courses';

import {
  QuizResultComponent,
  TakeQuizComponent
} from '@frontend/features/assessment';

import { ManageUsersComponent } from '@frontend/features/admin';
import { DenialPage } from '@frontend/ui';

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

      { path: 'courses', component: ListCoursesComponent },
      { path: 'my-learning', component: MyLearningComponent },
      { path: 'courses/:id', component: CourseDetailsComponent },
      { path: 'courses/:id/modules', component: ModulesListComponent },
      { path: 'manage-courses', component: AdminManageCoursesComponent },
      { path: 'courses/edit/:id', component: EditCourseComponent },
      { path: 'courses/module/:moduleId', component: ModuleContentComponent },
      { path: 'quiz/:moduleId', component: TakeQuizComponent },
      { path: 'assessment/result/:submissionId', component: QuizResultComponent },
      { path: 'manage-users', component: ManageUsersComponent }
    ]
  },

  {
    path: '**',
    redirectTo: 'access-denied'
  }
];
