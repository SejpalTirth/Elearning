import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { EditCourseComponent } from './edit-course';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { CourseFacade, AssessmentFacade } from '@frontend/core';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

describe('EditCourseComponent', () => {
  let component: EditCourseComponent;
  let fixture: ComponentFixture<EditCourseComponent>;

  let routerMock = { navigate: jasmine.createSpy('navigate').and.returnValue(Promise.resolve(true)) };

  let courseApiMock: Partial<CourseFacade>;
  let assessmentApiMock: Partial<AssessmentFacade>;

  beforeEach(async () => {
    courseApiMock = {
      getCategories: jasmine.createSpy('getCategories').and.returnValue(of([{ id: 1, name: 'Cat1' }])),
      getCourseById: jasmine.createSpy('getCourseById').and.returnValue(of({
        id: 1,
        title: 'Test Course',
        description: 'desc',
        categoryId: 1,
        modules: [{ id: 1, title: 'M1', content: 'C1' }],
        isDeleted: false
      })),
      updateCourse: jasmine.createSpy('updateCourse').and.returnValue(of({}))
    };

    assessmentApiMock = {
      getQuizStatusForCourse: jasmine.createSpy('getQuizStatusForCourse').and.returnValue(of({ allQuizzesCreated: true }))
    };

    await TestBed.configureTestingModule({
      imports: [EditCourseComponent],
      providers: [
        provideCommonMocks,
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: AssessmentFacade, useValue: assessmentApiMock },
        { provide: Router, useValue: routerMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EditCourseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loadCategories loads categories and course', fakeAsync(() => {
    component.loadCategories();
    tick();
    expect(courseApiMock.getCategories).toHaveBeenCalled();
    expect(courseApiMock.getCourseById).toHaveBeenCalledWith({ courseId: component.courseId });
    expect(component.categories.length).toBe(1);
    expect(component.courseData.title).toBe('Test Course');
  }));

  it('loadCourse handles deleted course', fakeAsync(() => {
    (courseApiMock.getCourseById as jasmine.Spy).and.returnValue(of({ isDeleted: true }));
    component.loadCourse();
    tick();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses']);
  }));

  it('addModule adds a new module', () => {
    const initialLength = component.modules.length;
    component.addModule();
    expect(component.modules.length).toBe(initialLength + 1);
  });

  it('removeModule removes a module', () => {
    component.addModule();
    const length = component.modules.length;
    component.removeModule(0);
    expect(component.modules.length).toBe(length - 1);
  });

  it('drop rearranges modules', () => {
    component.addModule();
    component.drop({ previousIndex: 0, currentIndex: 1, item: null, container: null, previousContainer: null } as any);
    expect(component.modules.controls.length).toBeGreaterThan(0);
  });

  it('save updates course successfully', fakeAsync(() => {
    component.save();
    tick();
    expect(courseApiMock.updateCourse).toHaveBeenCalled();
    expect(assessmentApiMock.getQuizStatusForCourse).toHaveBeenCalledWith({ courseId: component.courseId });
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses']);
  }));

  it('save handles quiz not all created', fakeAsync(() => {
    (assessmentApiMock.getQuizStatusForCourse as jasmine.Spy).and.returnValue(of({ allQuizzesCreated: false }));
    component.save();
    tick();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/assessment/pending']);
  }));

  it('save handles updateCourse error', fakeAsync(() => {
    (courseApiMock.updateCourse as jasmine.Spy).and.returnValue(throwError(() => ({})));
    component.save();
    tick();
    // No navigation expected, just error toast
  }));
});
