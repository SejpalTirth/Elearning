import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyLearningComponent } from './my-learning';

describe('MyLearning', () => {
  let component: MyLearningComponent;
  let fixture: ComponentFixture<MyLearningComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyLearningComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyLearningComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
