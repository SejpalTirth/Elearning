import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModuleContent } from './module-content';

describe('ModuleContent', () => {
  let component: ModuleContent;
  let fixture: ComponentFixture<ModuleContent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModuleContent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModuleContent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
