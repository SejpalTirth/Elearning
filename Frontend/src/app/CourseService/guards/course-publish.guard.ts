import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AssessmentApiService } from '../../AssessmentService/services/assessment-api';

@Injectable({
  providedIn: 'root'
})
export class CoursePublishGuard implements CanActivate {

  constructor(
    private assessmentApi: AssessmentApiService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {

    const courseId = Number(route.paramMap.get('id'));

    if (!courseId) return of(true);

    //  Check if this course still has pending modules without quizzes
    return this.assessmentApi.getUnquizzedModules(courseId).pipe(
      map((missing: number[]) => {
        if (missing && missing.length > 0) {
          //  Cannot access course yet → redirect to quiz builder
          this.router.navigate(['/assessment/add-quiz', courseId]);
          return false;
        }

        //  Course is ready → allow access
        return true;
      }),
      catchError(err => {
        console.error('CoursePublishGuard error:', err);
        return of(true);
      })
    );
  }
}
