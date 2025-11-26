import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModulesList } from './modules-list';

describe('ModulesList', () => {
  let component: ModulesList;
  let fixture: ComponentFixture<ModulesList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModulesList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModulesList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
