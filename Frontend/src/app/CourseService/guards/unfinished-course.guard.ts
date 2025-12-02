// src/app/CourseService/guards/unfinished-course.guard.ts
import { Injectable } from '@angular/core';
import {
  CanActivate,
  Router,
  UrlTree
} from '@angular/router';

import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UnfinishedCourseGuard implements CanActivate {

  constructor(private router: Router) {}

  canActivate(): Observable<boolean | UrlTree> {
    
    const data = localStorage.getItem('unfinishedCourse');
    if (!data) {
      // Nothing unfinished → allow access
      return of(true);
    }

    const course = JSON.parse(data);

    // Redirect to quiz creation
    return of(this.router.createUrlTree([
      '/assessment/add-quiz',
      course.courseId
    ]));
  }
}
