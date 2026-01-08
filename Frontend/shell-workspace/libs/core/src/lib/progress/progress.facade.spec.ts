import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { HttpResponse } from '@angular/common/http';

import { ProgressService } from './progress.facade';
import { ProgressGatewayService } from '@frontend/api';

describe('ProgressService', () => {
  let service: ProgressService;
  let gatewaySpy: jasmine.SpyObj<ProgressGatewayService>;

  beforeEach(() => {
    gatewaySpy = jasmine.createSpyObj('ProgressGatewayService', [
      'postApiProgressUser',
      'postApiProgressCompleteModule'
    ]);

    TestBed.configureTestingModule({
      providers: [
        ProgressService,
        { provide: ProgressGatewayService, useValue: gatewaySpy }
      ]
    });

    service = TestBed.inject(ProgressService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ===============================
  // LOAD PROGRESS
  // ===============================

  it('should load user progress and update progress$', (done) => {
    const progress = [
      { courseId: 1, moduleId: 2, isCompleted: true }
    ] as any;

    gatewaySpy.postApiProgressUser.and.returnValue(of(progress));

    service.loadUserProgress().subscribe(result => {
      expect(result).toEqual(progress);
    });

    service.progress$.subscribe(value => {
      expect(value).toEqual(progress);
      done();
    });

    expect(gatewaySpy.postApiProgressUser)
      .toHaveBeenCalledWith(jasmine.objectContaining({ withCredentials: true }));
  });

  it('should refresh progress by calling loadUserProgress', () => {
    spyOn(service, 'loadUserProgress').and.returnValue(of([]));

    service.refresh();

    expect(service.loadUserProgress).toHaveBeenCalled();
  });

  // ===============================
  // COMPLETE MODULE
  // ===============================

  it('should complete module and refresh progress', () => {
    const payload = { moduleId: 10 } as any;

    gatewaySpy.postApiProgressCompleteModule.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    spyOn(service, 'refresh');

    service.completeModule(payload).subscribe();

    expect(gatewaySpy.postApiProgressCompleteModule)
      .toHaveBeenCalledWith(
        payload,
        jasmine.objectContaining({ withCredentials: true })
      );

    expect(service.refresh).toHaveBeenCalled();
  });

  // ===============================
  // CLEAR
  // ===============================

  it('should clear progress state', (done) => {
    // preload state
    (service as any).progressSubject.next([
      { courseId: 1, moduleId: 1, isCompleted: true }
    ]);

    service.clear();

    service.progress$.subscribe(value => {
      expect(value).toEqual([]);
      done();
    });
  });
});
