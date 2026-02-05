import { TestBed } from '@angular/core/testing';
import { LoadingService } from './loading-service';

describe('LoadingService', () => {
  let service: LoadingService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LoadingService]
    });
    service = TestBed.inject(LoadingService);
  });

  it('should be created with initial loading state as false', (done) => {
    service.loading$.subscribe(isLoading => {
      expect(isLoading).toBeFalse();
      done();
    });
  });

  it('should emit true when show() is called', (done) => {
    service.show();
    service.loading$.subscribe(isLoading => {
      expect(isLoading).toBeTrue();
      done();
    });
  });

  it('should emit false when hide() is called', (done) => {
    service.show();
    
    service.hide();
    service.loading$.subscribe(isLoading => {
      expect(isLoading).toBeFalse();
      done();
    });
  });
});