import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AdminManageCoursesComponent } from './admin-manage-courses';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { CourseFacade } from '@frontend/core';
import { Router } from '@angular/router';
import { ToastService, LoadingService } from '@frontend/ui';
import { of, throwError } from 'rxjs';

describe('AdminManageCoursesComponent', () => {
  let component: AdminManageCoursesComponent;
  let fixture: ComponentFixture<AdminManageCoursesComponent>;

  let routerMock = { navigate: jasmine.createSpy('navigate') };
  let courseApiMock: Partial<CourseFacade>;
  let toastMock: Partial<ToastService>;
  let loaderMock: Partial<LoadingService>;

  beforeEach(async () => {
    courseApiMock = {
      getAllCourses: jasmine.createSpy('getAllCourses').and.returnValue(of([{ id: 1, title: 'Test Course' }])),
      deleteCourse: jasmine.createSpy('deleteCourse').and.returnValue(of({})),
      restoreCourse: jasmine.createSpy('restoreCourse').and.returnValue(of({}))
    };

    toastMock = { showSuccess: jasmine.createSpy('showSuccess') };
    loaderMock = { show: jasmine.createSpy('show'), hide: jasmine.createSpy('hide') };

    await TestBed.configureTestingModule({
      imports: [AdminManageCoursesComponent],
      providers: [
        provideCommonMocks,
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: Router, useValue: routerMock },
        { provide: ToastService, useValue: toastMock },
        { provide: LoadingService, useValue: loaderMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminManageCoursesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit should call loadCourses and update signal', fakeAsync(() => {
    component.ngOnInit();
    tick();
    expect(courseApiMock.getAllCourses).toHaveBeenCalled();
    expect(component.courses()).toEqual([{ id: 1, title: 'Test Course' }]);
    expect(component.loading()).toBe(false);
  }));

  it('editCourse should navigate if not deleted', () => {
    component.editCourse(1, false);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses/edit/1']);
  });

  it('editCourse should not navigate if deleted', () => {
    component.editCourse(1, true);
    expect(routerMock.navigate).not.toHaveBeenCalled();
  });

  it('deleteCourse should confirm and call API', fakeAsync(() => {
    spyOn(window, 'confirm').and.returnValue(true);

    component.deleteCourse(1);
    tick();

    expect(courseApiMock.deleteCourse).toHaveBeenCalledWith({ courseId: 1 });
    expect(toastMock.showSuccess).toHaveBeenCalledWith('Course archived (soft deleted) successfully!');
    expect(loaderMock.show).toHaveBeenCalled();
    expect(loaderMock.hide).toHaveBeenCalled();
  }));

  it('deleteCourse should not call API if confirm is false', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    component.deleteCourse(1);
    expect(courseApiMock.deleteCourse).not.toHaveBeenCalled();
  });

  it('restoreCourse should call API and show toast', fakeAsync(() => {
    component.restoreCourse(1);
    tick();
    expect(courseApiMock.restoreCourse).toHaveBeenCalledWith({ courseId: 1 });
    expect(toastMock.showSuccess).toHaveBeenCalledWith('Course restored successfully!');
    expect(loaderMock.show).toHaveBeenCalled();
    expect(loaderMock.hide).toHaveBeenCalled();
  }));

  it('trackById should return item.id', () => {
    const item = { id: 99 };
    expect(component.trackById(0, item)).toBe(99);
  });

  it('loadCourses should handle error', fakeAsync(() => {
    (courseApiMock.getAllCourses as jasmine.Spy).and.returnValue(throwError(() => new Error('Fail')));
    spyOn(console, 'error');
    component.loadCourses();
    tick();
    expect(console.error).toHaveBeenCalled();
  }));
});
