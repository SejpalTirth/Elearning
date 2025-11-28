import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModulesListComponent } from './modules-list';

describe('ModulesList', () => {
  let component: ModulesListComponent;
  let fixture: ComponentFixture<ModulesListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModulesListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModulesListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
