import { Routes } from '@angular/router';

export const COURSE_ROUTES: Routes = [

  // Student learning area
  { path: 'my-learning', loadComponent: () => import('./my-learning/my-learning').then(m => m.MyLearningComponent) },
  { path: 'module/:moduleId', loadComponent: () => import('./module-content/module-content').then(m => m.ModuleContentComponent) },
  { path: ':id/modules', loadComponent: () => import('./modules-list/modules-list').then(m => m.ModulesListComponent) },

  // Add course (Instructor)
  { path: 'add/new', loadComponent: () => import('./add/add-course').then(m => m.AddCourseComponent) },

  // Edit existing course (Instructor)
  { path: 'edit/:id', loadComponent: () => import('./edit/edit').then(m => m.EditCourseComponent) },

  // Instructor manage OWN courses
  { path: 'manage', loadComponent: () => import('./manage/manage').then(m => m.ManageCoursesComponent) },

  // Admin manage ALL courses (delete allowed)
  { path: 'manage-courses', loadComponent: () => import('./manage-courses/manage-courses').then(m => m.ManageCoursesComponent) },

  // Default list page for public view
  { path: '', loadComponent: () => import('./list/list-courses').then(m => m.ListCoursesComponent) },

  // Course details last (avoid route conflict)
  { path: ':id', loadComponent: () => import('./details/course-details').then(m => m.CourseDetailsComponent) }
];
