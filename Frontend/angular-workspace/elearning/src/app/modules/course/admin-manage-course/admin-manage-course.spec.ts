import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminManageCoursesComponent } from './admin-manage-course';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { GatewayCourseService } from 'api';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { LoadingService } from '../../../common-modules/ui/loading/loading-service';

describe('AdminManageCoursesComponent', () => {
  let component: AdminManageCoursesComponent;
  let fixture: ComponentFixture<AdminManageCoursesComponent>;
  
  let mockRouter: any;
  let mockToast: any;
  let mockLoader: any;
  let mockApi: any;

  const mockCourses = [
    { id: 1, title: 'Angular 101', isDeleted: false },
    { id: 2, title: 'Archived Course', isDeleted: true }
  ];

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockToast = { showSuccess: jasmine.createSpy('showSuccess') };
    mockLoader = { show: jasmine.createSpy('show'), hide: jasmine.createSpy('hide') };
    mockApi = {
      postApiCourseAll: jasmine.createSpy('postApiCourseAll').and.returnValue(of(mockCourses)),
      postApiCourseDelete: jasmine.createSpy('postApiCourseDelete').and.returnValue(of({})),
      postApiCourseRestore: jasmine.createSpy('postApiCourseRestore').and.returnValue(of({}))
    };

    await TestBed.configureTestingModule({
      imports: [AdminManageCoursesComponent],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ToastService, useValue: mockToast },
        { provide: LoadingService, useValue: mockLoader },
        { provide: GatewayCourseService, useValue: mockApi }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminManageCoursesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges(); // Triggers ngOnInit -> loadCourses()
  });

  it('should load courses on init using signals', () => {
    expect(mockApi.postApiCourseAll).toHaveBeenCalled();
    // Accessing signal value
    expect(component.courses()).toEqual(mockCourses);
    expect(component.loading()).toBeFalse();
  });

  describe('editCourse', () => {
    it('should navigate if course is not deleted', () => {
      component.editCourse(1, false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses/edit/1']);
    });

    it('should NOT navigate if course is deleted', () => {
      component.editCourse(2, true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  describe('deleteCourse', () => {
    it('should call delete API if user confirms', () => {
      // Mocking the global confirm dialog
      spyOn(window, 'confirm').and.returnValue(true);

      component.deleteCourse(1);

      expect(mockApi.postApiCourseDelete).toHaveBeenCalledWith({ courseId: 1 });
      expect(mockToast.showSuccess).toHaveBeenCalled();
      // Verifying that it reloads the list
      expect(mockApi.postApiCourseAll).toHaveBeenCalledTimes(2); 
    });

    it('should cancel delete if user declines confirm', () => {
      spyOn(window, 'confirm').and.returnValue(false);
      component.deleteCourse(1);
      expect(mockApi.postApiCourseDelete).not.toHaveBeenCalled();
    });
  });

  describe('restoreCourse', () => {
    it('should call restore API and reload', () => {
      component.restoreCourse(2);
      expect(mockApi.postApiCourseRestore).toHaveBeenCalledWith({ courseId: 2 });
      expect(mockToast.showSuccess).toHaveBeenCalledWith('Course restored successfully!');
      expect(mockApi.postApiCourseAll).toHaveBeenCalledTimes(2);
    });
  });
});