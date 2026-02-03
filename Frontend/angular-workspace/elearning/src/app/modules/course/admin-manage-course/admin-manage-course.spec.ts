import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminManageCourse } from './admin-manage-course';

describe('AdminManageCourse', () => {
  let component: AdminManageCourse;
  let fixture: ComponentFixture<AdminManageCourse>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminManageCourse]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminManageCourse);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
