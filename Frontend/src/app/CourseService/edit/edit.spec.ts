import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditCourseComponent } from './edit';

describe('Edit', () => {
  let component: EditCourseComponent;
  let fixture: ComponentFixture<EditCourseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditCourseComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditCourseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
