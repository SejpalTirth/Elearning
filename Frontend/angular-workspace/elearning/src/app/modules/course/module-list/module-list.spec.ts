import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModuleList } from './module-list';

describe('ModuleList', () => {
  let component: ModuleList;
  let fixture: ComponentFixture<ModuleList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModuleList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModuleList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
