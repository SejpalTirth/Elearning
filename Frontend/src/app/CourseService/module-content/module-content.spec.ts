import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModuleContentComponent } from './module-content';

describe('ModuleContent', () => {
  let component: ModuleContentComponent;
  let fixture: ComponentFixture<ModuleContentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModuleContentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModuleContentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
