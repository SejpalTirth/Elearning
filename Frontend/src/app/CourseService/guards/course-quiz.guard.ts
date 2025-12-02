// src/app/CourseService/guards/course-quiz.guard.ts
import { Injectable } from '@angular/core';
import {
  CanDeactivate,
  UrlTree,
  Router
} from '@angular/router';
import { Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { catchError, map } from 'rxjs/operators';

/**
 * CanDeactivate guard used for AddCourseComponent and EditCourseComponent.
 * - Expects the component to expose a numeric `courseId` property (or a getCourseId() method).
 * - Calls AssessmentService endpoint to find modules without quiz for that course.
 * - If there are missing modules, redirect to the assessment add-quiz page and block navigation.
 */
export interface CourseEditor {
  // Guard will try component.courseId first, then component.getCourseId()
  courseId?: number;
  getCourseId?: () => number | undefined;
}

@Injectable({
  providedIn: 'root'
})
export class CourseQuizGuard implements CanDeactivate<CourseEditor> {

  // Make sure this matches your AssessmentService base URL + endpoint
  private assessmentUnquizzedUrl = 'https://localhost:7249/api/assessment/unquizzed-modules'; 
  // If you use different base or gateway, update the above string.

  constructor(private http: HttpClient, private router: Router) {}

  canDeactivate(
    component: CourseEditor
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    // Determine courseId from component
    const courseId =
      (component && (component.courseId ?? (component.getCourseId ? component.getCourseId() : undefined)))
      ?? undefined;

    // If no courseId available, allow navigation
    if (!courseId || isNaN(courseId)) {
      return true;
    }

    // Call AssessmentService to get missing module ids
    const url = `${this.assessmentUnquizzedUrl}/${courseId}`;

    return this.http.get<number[]>(url).pipe(
      map((missingModuleIds: number[]) => {
        // If the list is empty => all quizzes created => allow navigation
        if (!missingModuleIds || missingModuleIds.length === 0) {
          return true;
        }

        // There are modules missing quizzes -> redirect to Add-Quiz page for this course
        // We assume your add-quiz route is '/assessment/add-quiz/:courseId'
        // Adjust the route if your route is different.
        this.router.navigate(['/assessment/add-quiz', courseId]);
        return false;
      }),
      catchError((err) => {
        // On error contacting AssessmentService, be conservative: allow navigation
        // (alternatively you can block navigation and show an error)
        console.error('CourseQuizGuard: failed to check quiz status', err);
        return of(true);
      })
    );
  }
}
