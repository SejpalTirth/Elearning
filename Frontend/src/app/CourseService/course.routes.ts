import { Routes } from '@angular/router';
import { CourseQuizGuard } from './guards/course-quiz.guard';
import { UnfinishedCourseGuard } from './guards/unfinished-course.guard';

export const COURSE_ROUTES: Routes = [

  // ---------------- STUDENT ----------------
  {
    path: 'my-learning',
    loadComponent: () =>
      import('./my-learning/my-learning')
        .then(m => m.MyLearningComponent)
  },

  {
    path: 'module/:moduleId',
    loadComponent: () =>
      import('./module-content/module-content')
        .then(m => m.ModuleContentComponent)
  },

  {
    path: ':id/modules',
    loadComponent: () =>
      import('./modules-list/modules-list')
        .then(m => m.ModulesListComponent)
  },

  // ---------------- INSTRUCTOR ----------------
  {
    path: 'add/new',
    loadComponent: () =>
      import('./add/add-course')
        .then(m => m.AddCourseComponent),
    canActivate: [UnfinishedCourseGuard],
    canDeactivate: [CourseQuizGuard]
  },

  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./edit/edit')
        .then(m => m.EditCourseComponent),
    canActivate: [UnfinishedCourseGuard]
  },

  {
    path: 'manage',
    loadComponent: () =>
      import('./manage/manage')
        .then(m => m.ManageCoursesComponent)
  },

  {
    path: 'manage-courses',
    loadComponent: () =>
      import('./manage-courses/manage-courses')
        .then(m => m.ManageCoursesComponent)
  },

  // ---------------- PUBLIC ----------------
  {
    path: '',
    loadComponent: () =>
      import('./list/list-courses')
        .then(m => m.ListCoursesComponent)
  },

  {
    path: ':id',
    loadComponent: () =>
      import('./details/course-details')
        .then(m => m.CourseDetailsComponent)
  }
];
