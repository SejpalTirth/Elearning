import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { ModulesListComponent } from './modules-list';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { CourseFacade, ProgressService } from '@frontend/core';
import { AuthStateService, AuthUser } from '@frontend/auth';
import { Router } from '@angular/router';
import { of, BehaviorSubject } from 'rxjs';

describe('ModulesListComponent', () => {
  let component: ModulesListComponent;
  let fixture: ComponentFixture<ModulesListComponent>;

  let courseApiMock: Partial<CourseFacade>;
  let progressServiceMock: Partial<ProgressService>;
  let authStateMock: Partial<AuthStateService>;
  let routerMock: Partial<Router>;

  beforeEach(waitForAsync(() => {
    // Mocks
    courseApiMock = {
      getCourseModules: jasmine.createSpy('getCourseModules').and.returnValue(
        of([
          { id: 1, title: 'Module 1' },
          { id: 2, title: 'Module 2' }
        ])
      )
    };

    progressServiceMock = {
      loadUserProgress: jasmine.createSpy('loadUserProgress').and.returnValue(of(null)),
      progress$: new BehaviorSubject([
        { courseId: 101, moduleId: 1, progressPercent: 50, isCompleted: false },
        { courseId: 101, moduleId: 2, progressPercent: 100, isCompleted: true }
      ])
    };

    const fakeUser: AuthUser = {
      userId: 'user-1',
      email: 'test@test.com',
      name: 'Test User',
      role: 'Student' // must match exact union type
    };

    authStateMock = {
      user$: of(fakeUser)
    };

    routerMock = {
      navigate: jasmine.createSpy('navigate')
    };

    TestBed.configureTestingModule({
      imports: [ModulesListComponent],
      providers: [
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: ProgressService, useValue: progressServiceMock },
        { provide: AuthStateService, useValue: authStateMock },
        { provide: Router, useValue: routerMock },
        ...provideCommonMocks
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ModulesListComponent);
    component = fixture.componentInstance;
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load modules and bind progress', () => {
    component.courseId = 101;
    component.ngOnInit();
    fixture.detectChanges();

    expect(courseApiMock.getCourseModules).toHaveBeenCalledWith({ courseId: 101 });
    expect(progressServiceMock.loadUserProgress).toHaveBeenCalled();

    expect(component.modules.length).toBe(2);
    expect(component.modules[0].progressPercent).toBe(50);
    expect(component.modules[0].isCompleted).toBe(false);
    expect(component.modules[1].progressPercent).toBe(100);
    expect(component.modules[1].isCompleted).toBe(true);

    expect(component.loading).toBeFalse();
  });

  it('should navigate to module when openModule is called', () => {
    component.openModule(1);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses/module/1']);
  });

  it('should handle null user', () => {
    authStateMock.user$ = of(null);
    component.ngOnInit();
    fixture.detectChanges();

    expect(component.modules).toEqual([]);
    expect(component.loading).toBeFalse();
  });

});
