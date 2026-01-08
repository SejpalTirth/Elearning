import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  GatewayCourseService,
  GatewayContractsCourseCourse,
  GatewayContractsCourseCourseIdRequest,
  GatewayContractsCourseContinueCourseRequest,
  GatewayContractsCourseEnrollRequest,
  GatewayContractsCourseModuleIdRequest,
  GatewayContractsCourseUpdateCourseRequest
} from '@frontend/api';

@Injectable({ providedIn: 'root' })
export class CourseFacade {

  private readonly api = inject(GatewayCourseService);

  // --------------------------------------------------
  // QUERY
  // --------------------------------------------------

  getAllCourses(): Observable<any> {
    return this.api.postApiCourseAll();
  }

  getCourseById(
    payload: GatewayContractsCourseCourseIdRequest
  ): Observable<any> {
    return this.api.postApiCourseById(payload);
  }

  getInstructorCourses(): Observable<any> {
    return this.api.postApiCourseInstructor();
  }

  getUnfinishedCourses(): Observable<any> {
    return this.api.postApiCourseUnfinished();
  }

  getEnrolledCourses(): Observable<any> {
    return this.api.postApiCourseEnrolled();
  }

  getCourseModules(
    payload: GatewayContractsCourseCourseIdRequest
  ): Observable<any> {
    return this.api.postApiCourseModules(payload);
  }

  getCourseModule(
    payload: GatewayContractsCourseModuleIdRequest
  ): Observable<any> {
    return this.api.postApiCourseModule(payload);
  }

  getCategories(): Observable<any> {
    return this.api.postApiCourseCategories();
  }

  // --------------------------------------------------
  // COMMANDS
  // --------------------------------------------------

  createCourse(
    payload: GatewayContractsCourseCourse
  ): Observable<void> {
    return this.api.postApiCourseCreate(payload);
  }

  updateCourse(
    payload: GatewayContractsCourseUpdateCourseRequest
  ): Observable<void> {
    return this.api.postApiCourseUpdate(payload);
  }

  deleteCourse(
    payload: GatewayContractsCourseCourseIdRequest
  ): Observable<void> {
    return this.api.postApiCourseDelete(payload);
  }

  publishCourse(
    payload: GatewayContractsCourseCourseIdRequest
  ): Observable<void> {
    return this.api.postApiCoursePublish(payload);
  }

  restoreCourse(
    payload: GatewayContractsCourseCourseIdRequest
  ): Observable<void> {
    return this.api.postApiCourseRestore(payload);
  }

  enroll(
    payload: GatewayContractsCourseEnrollRequest
  ): Observable<void> {
    return this.api.postApiCourseEnroll(payload);
  }

  continueCourse(
    payload: GatewayContractsCourseContinueCourseRequest
  ): Observable<void> {
    return this.api.postApiCourseContinue(payload);
  }
}
