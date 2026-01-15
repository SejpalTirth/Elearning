import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ListCoursesComponent } from './list-course';
import { provideCommonMocks } from '../../../../../../test-utils/mocks';
import { CourseFacade } from '@frontend/core';
import { Router } from '@angular/router';
import { of } from 'rxjs';

describe('ListCoursesComponent', () => {
  let component: ListCoursesComponent;
  let fixture: ComponentFixture<ListCoursesComponent>;
  let courseApiMock: Partial<CourseFacade>;
  let routerMock: Partial<Router>;

  beforeEach(async () => {
    courseApiMock = {
      getAllCourses: jasmine.createSpy('getAllCourses').and.returnValue(of([
        { id: 1, title: 'Course 1', isDeleted: false, isDraft: false },
        { id: 2, title: 'Course 2', isDeleted: true, isDraft: false },
        { id: 3, title: 'Course 3', isDeleted: false, isDraft: true }
      ]))
    };

    routerMock = { navigate: jasmine.createSpy('navigate').and.returnValue(Promise.resolve(true)) };

    await TestBed.configureTestingModule({
      providers: [
        provideCommonMocks,
        { provide: CourseFacade, useValue: courseApiMock },
        { provide: Router, useValue: routerMock }
      ],
      declarations: [ListCoursesComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ListCoursesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load and filter courses', fakeAsync(() => {
    component.ngOnInit();
    tick();
    expect(courseApiMock.getAllCourses).toHaveBeenCalled();
    expect(component.courses.length).toBe(1);
    expect(component.courses[0].id).toBe(1);
    expect(component.loading).toBeFalse();
  }));

  it('openCourse should navigate to course', () => {
    component.openCourse(1);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/courses', 1]);
  });

  it('checkAllOverflows should mark isTruncated', () => {
    // Mock DOM elements
    const paragraph = document.createElement('p');
    paragraph.classList.add('description');
    Object.defineProperty(paragraph, 'scrollHeight', { value: 100 });
    Object.defineProperty(paragraph, 'clientHeight', { value: 50 });

    document.body.appendChild(paragraph);
    component.courses = [{ id: 1 }];
    (component as any).checkAllOverflows();

    expect(component.courses[0].isTruncated).toBeTrue();

    document.body.removeChild(paragraph);
  });
});
