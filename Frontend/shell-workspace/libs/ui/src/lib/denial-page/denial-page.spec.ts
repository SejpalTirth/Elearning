import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DenialPage } from './denial-page';

describe('DenialPage', () => {
  let component: DenialPage;
  let fixture: ComponentFixture<DenialPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DenialPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DenialPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
