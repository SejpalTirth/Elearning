import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ListCoursesComponent } from './list-course';
import { Router } from '@angular/router';
import { GatewayCourseService } from 'api';
import { of, throwError } from 'rxjs';

describe('ListCoursesComponent', () => {
  let component: ListCoursesComponent;
  let fixture: ComponentFixture<ListCoursesComponent>;
  let mockRouter: any;
  let mockCourseApi: any;

  const mockCourses = [
    { id: 1, title: 'Active', isDeleted: false, isDraft: false },
    { id: 2, title: 'Deleted', isDeleted: true, isDraft: false },
    { id: 3, title: 'Draft', isDeleted: false, isDraft: true }
  ];

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockCourseApi = {
      postApiCourseAll: jasmine.createSpy('postApiCourseAll').and.returnValue(of(mockCourses))
    };

    await TestBed.configureTestingModule({
      imports: [ListCoursesComponent],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: GatewayCourseService, useValue: mockCourseApi }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ListCoursesComponent);
    component = fixture.componentInstance;
  });

  it('should filter out deleted and draft courses on load', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    expect(mockCourseApi.postApiCourseAll).toHaveBeenCalled();
    expect(component.courses.length).toBe(1);
    expect(component.courses[0].id).toBe(1);
    expect(component.loading).toBeFalse();
  }));

  it('should navigate to course details when opening a course', () => {
    component.openCourse(123);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses', 123]);
  });

  it('should handle API error gracefully', () => {
    mockCourseApi.postApiCourseAll.and.returnValue(throwError(() => new Error('Error')));
    fixture.detectChanges();

    expect(component.loading).toBeFalse();
    expect(component.courses).toEqual([]);
  });

  it('should detect text overflow after courses are rendered', fakeAsync(() => {
    const mockElement = document.createElement('p');
    Object.defineProperty(mockElement, 'scrollHeight', { value: 100 });
    Object.defineProperty(mockElement, 'clientHeight', { value: 50 });
    
    spyOn(document, 'querySelectorAll').and.returnValue([mockElement] as any);

    fixture.detectChanges();
    tick();

    expect(component.courses[0].isTruncated).toBeTrue();
  }));
});