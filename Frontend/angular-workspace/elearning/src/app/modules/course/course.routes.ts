import { Routes } from '@angular/router';

export const courseRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./list-course/list-course')
        .then(m => m.ListCoursesComponent),
  },
  {
    path: 'manage-courses',
    loadComponent: () =>
      import('./admin-manage-course/admin-manage-course')
        .then(m => m.AdminManageCoursesComponent),
  },
  {
    path: 'my-courses',
    loadComponent: () =>
      import('./instructor-course-list/instructor-course-list')
        .then(m => m.InstructorCourseList),
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./add-course/add-course')
        .then(m => m.AddCourseComponent),
  },
  {
    path: 'my-learning',
    loadComponent: () =>
      import('./my-learning/my-learning')
        .then(m => m.MyLearningComponent),
  },
  {
    path: 'module/:moduleId',
    loadComponent: () =>
      import('./module-content/module-content')
        .then(m => m.ModuleContentComponent),
  },
  {
    path: ':id/modules',
    loadComponent: () =>
      import('./module-list/module-list')
        .then(m => m.ModulesListComponent),
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./edit-course/edit-course')
        .then(m => m.EditCourseComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./course-details/course-details')
        .then(m => m.CourseDetailsComponent),
  },
];
