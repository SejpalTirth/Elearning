import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { CompletedComponent } from './completed';

describe('CompletedComponent', () => {

  let component: CompletedComponent;
  let fixture: ComponentFixture<CompletedComponent>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [CompletedComponent],
      providers: [
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CompletedComponent);
    component = fixture.componentInstance;
  });

  // ---------------------------------------------------------
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ---------------------------------------------------------
  it('should load quizTitle from history.state', () => {
    // Mock navigation state
    history.replaceState({ quizTitle: 'Java Basics Quiz' }, '');

    component.ngOnInit();

    expect(component.quizTitle).toBe('Java Basics Quiz');
  });

  // ---------------------------------------------------------
  it('should fallback to default title when no state found', () => {
    history.replaceState({}, '');

    component.ngOnInit();

    expect(component.quizTitle).toBe('this module');
  });

  // ---------------------------------------------------------
  it('should navigate back to my-learning', () => {
    component.goBack();

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/my-learning']);
  });

});
