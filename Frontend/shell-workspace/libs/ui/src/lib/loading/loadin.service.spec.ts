import { TestBed } from '@angular/core/testing';
import { LoadingService } from './loading.service';

describe('LoadingService', () => {
  let service: LoadingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LoadingService);
  });

  it('should start with loading=false', (done) => {
    service.loading$.subscribe(value => {
      expect(value).toBe(false);
      done();
    });
  });

  it('should set loading=true on show()', (done) => {
    service.show();
    service.loading$.subscribe(value => {
      expect(value).toBe(true);
      done();
    });
  });

  it('should set loading=false on hide()', (done) => {
    service.show(); // first set to true
    service.hide();
    service.loading$.subscribe(value => {
      expect(value).toBe(false);
      done();
    });
  });
});
