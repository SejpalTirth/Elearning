import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListCourses } from './list-courses';

describe('ListCourses', () => {
  let component: ListCourses;
  let fixture: ComponentFixture<ListCourses>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListCourses]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListCourses);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
