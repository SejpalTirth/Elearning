import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { HttpResponse } from '@angular/common/http';

import { CourseFacade } from './course.facade';
import { GatewayCourseService } from '@frontend/api';

describe('CourseFacade', () => {
  let facade: CourseFacade;
  let gatewaySpy: jasmine.SpyObj<GatewayCourseService>;

  beforeEach(() => {
    gatewaySpy = jasmine.createSpyObj('GatewayCourseService', [
      'postApiCourseAll',
      'postApiCourseById',
      'postApiCourseInstructor',
      'postApiCourseUnfinished',
      'postApiCourseEnrolled',
      'postApiCourseModules',
      'postApiCourseModule',
      'postApiCourseCategories',
      'postApiCourseCreate',
      'postApiCourseUpdate',
      'postApiCourseDelete',
      'postApiCoursePublish',
      'postApiCourseRestore',
      'postApiCourseEnroll',
      'postApiCourseContinue'
    ]);

    TestBed.configureTestingModule({
      providers: [
        CourseFacade,
        { provide: GatewayCourseService, useValue: gatewaySpy }
      ]
    });

    facade = TestBed.inject(CourseFacade);
  });

  it('should be created', () => {
    expect(facade).toBeTruthy();
  });

  // --------------------------------------------------
  // QUERY
  // --------------------------------------------------

  it('should get all courses', () => {
    const body = [{ id: 1, title: 'Course 1' }];

    gatewaySpy.postApiCourseAll.and.returnValue(
      of(new HttpResponse({ body }))
    );

    facade.getAllCourses().subscribe(res => {
      expect(res.body).toEqual(body);
    });

    expect(gatewaySpy.postApiCourseAll).toHaveBeenCalled();
  });

  it('should get course by id', () => {
    const payload = { courseId: 1 } as any;
    const body = { id: 1, title: 'Course 1' };

    gatewaySpy.postApiCourseById.and.returnValue(
      of(new HttpResponse({ body }))
    );

    facade.getCourseById(payload).subscribe(res => {
      expect(res.body).toEqual(body);
    });

    expect(gatewaySpy.postApiCourseById).toHaveBeenCalledWith(payload);
  });

  it('should get instructor courses', () => {
    const body = [{ id: 1 }];

    gatewaySpy.postApiCourseInstructor.and.returnValue(
      of(new HttpResponse({ body }))
    );

    facade.getInstructorCourses().subscribe(res => {
      expect(res.body).toEqual(body);
    });

    expect(gatewaySpy.postApiCourseInstructor).toHaveBeenCalled();
  });

  it('should get unfinished courses', () => {
    const body = [{ id: 2 }];

    gatewaySpy.postApiCourseUnfinished.and.returnValue(
      of(new HttpResponse({ body }))
    );

    facade.getUnfinishedCourses().subscribe(res => {
      expect(res.body).toEqual(body);
    });

    expect(gatewaySpy.postApiCourseUnfinished).toHaveBeenCalled();
  });

  it('should get enrolled courses', () => {
    const body = [{ id: 3 }];

    gatewaySpy.postApiCourseEnrolled.and.returnValue(
      of(new HttpResponse({ body }))
    );

    facade.getEnrolledCourses().subscribe(res => {
      expect(res.body).toEqual(body);
    });

    expect(gatewaySpy.postApiCourseEnrolled).toHaveBeenCalled();
  });

  it('should get course modules', () => {
    const payload = { courseId: 1 } as any;
    const body = [{ moduleId: 10 }];

    gatewaySpy.postApiCourseModules.and.returnValue(
      of(new HttpResponse({ body }))
    );

    facade.getCourseModules(payload).subscribe(res => {
      expect(res.body).toEqual(body);
    });

    expect(gatewaySpy.postApiCourseModules).toHaveBeenCalledWith(payload);
  });

  it('should get course module', () => {
    const payload = { moduleId: 10 } as any;
    const body = { moduleId: 10, title: 'Module' };

    gatewaySpy.postApiCourseModule.and.returnValue(
      of(new HttpResponse({ body }))
    );

    facade.getCourseModule(payload).subscribe(res => {
      expect(res.body).toEqual(body);
    });

    expect(gatewaySpy.postApiCourseModule).toHaveBeenCalledWith(payload);
  });

  it('should get categories', () => {
    const body = ['Frontend', 'Backend'];

    gatewaySpy.postApiCourseCategories.and.returnValue(
      of(new HttpResponse({ body }))
    );

    facade.getCategories().subscribe(res => {
      expect(res.body).toEqual(body);
    });

    expect(gatewaySpy.postApiCourseCategories).toHaveBeenCalled();
  });

  // --------------------------------------------------
  // COMMANDS
  // --------------------------------------------------

  it('should create course', () => {
    const payload = { title: 'New Course' } as any;

    gatewaySpy.postApiCourseCreate.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.createCourse(payload).subscribe();

    expect(gatewaySpy.postApiCourseCreate).toHaveBeenCalledWith(payload);
  });

  it('should update course', () => {
    const payload = { courseId: 1, title: 'Updated' } as any;

    gatewaySpy.postApiCourseUpdate.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.updateCourse(payload).subscribe();

    expect(gatewaySpy.postApiCourseUpdate).toHaveBeenCalledWith(payload);
  });

  it('should delete course', () => {
    const payload = { courseId: 1 } as any;

    gatewaySpy.postApiCourseDelete.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.deleteCourse(payload).subscribe();

    expect(gatewaySpy.postApiCourseDelete).toHaveBeenCalledWith(payload);
  });

  it('should publish course', () => {
    const payload = { courseId: 1 } as any;

    gatewaySpy.postApiCoursePublish.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.publishCourse(payload).subscribe();

    expect(gatewaySpy.postApiCoursePublish).toHaveBeenCalledWith(payload);
  });

  it('should restore course', () => {
    const payload = { courseId: 1 } as any;

    gatewaySpy.postApiCourseRestore.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.restoreCourse(payload).subscribe();

    expect(gatewaySpy.postApiCourseRestore).toHaveBeenCalledWith(payload);
  });

  it('should enroll in course', () => {
    const payload = { courseId: 1 } as any;

    gatewaySpy.postApiCourseEnroll.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.enroll(payload).subscribe();

    expect(gatewaySpy.postApiCourseEnroll).toHaveBeenCalledWith(payload);
  });

  it('should continue course', () => {
    const payload = { courseId: 1, moduleId: 2 } as any;

    gatewaySpy.postApiCourseContinue.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.continueCourse(payload).subscribe();

    expect(gatewaySpy.postApiCourseContinue).toHaveBeenCalledWith(payload);
  });
});
