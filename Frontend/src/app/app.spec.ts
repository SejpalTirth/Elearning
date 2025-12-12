import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { Router, NavigationEnd } from '@angular/router';
import { Subject } from 'rxjs';

// Mock Router with event stream
class MockRouter {
  public events = new Subject<any>();
}

describe('App Component', () => {

  let router: MockRouter;

  beforeEach(async () => {
    router = new MockRouter();

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        { provide: Router, useValue: router }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should update currentUrl on navigation', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    // Simulate navigation end event
    router.events.next(new NavigationEnd(1, '/dashboard', '/dashboard'));

    expect(app.currentUrl).toBe('/dashboard');
  });

  it('should detect login page correctly', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    router.events.next(new NavigationEnd(1, '/', '/'));

    expect(app.isLoginPage()).toBeTrue();
  });

});
