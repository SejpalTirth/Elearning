import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListCourse } from './list-course';

describe('ListCourse', () => {
  let component: ListCourse;
  let fixture: ComponentFixture<ListCourse>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListCourse]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListCourse);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
