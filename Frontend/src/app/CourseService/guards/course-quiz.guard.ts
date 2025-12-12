import { inject, Injectable } from '@angular/core';
import {
  CanDeactivate,
  UrlTree,
  Router
} from '@angular/router';
import { Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { catchError, map } from 'rxjs/operators';
import { environment } from 'Environment/environment';

export interface CourseEditor {
  // Guard will try component.courseId first, then component.getCourseId()
  courseId?: number;
  getCourseId?: () => number | undefined;
}

@Injectable({
  providedIn: 'root'
})
export class CourseQuizGuard implements CanDeactivate<CourseEditor> {

  private assessmentUnquizzedUrl = `${environment.baseapiurl}/AssessmentGateway/unquizzed-modules`;

  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  canDeactivate(
    component: CourseEditor
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    const courseId =
      (component && (component.courseId ?? (component.getCourseId ? component.getCourseId() : undefined)))
      ?? undefined;

    if (!courseId || isNaN(courseId)) {
      return true;
    }

    const url = `${this.assessmentUnquizzedUrl}/${courseId}`;

    return this.http.get<number[]>(url).pipe(
      map((missingModuleIds: number[]) => {
        if (!missingModuleIds || missingModuleIds.length === 0) {
          return true;
        }

        this.router.navigate(['/assessment/add-quiz', courseId]);
        return false;
      }),
      catchError(() => {
        return of(true);
      })
    );
  }
}
