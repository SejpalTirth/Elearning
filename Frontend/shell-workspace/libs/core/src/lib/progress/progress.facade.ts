import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import {
  ProgressGatewayService,
  GatewayContractsProgressModuleCompleteRequest,
  ProgressGatewayControllerProgressRecordDto
} from '@frontend/api';

export interface ProgressRecord {
  courseId: number;
  moduleId: number | null;
  isCompleted: boolean;
}

@Injectable({ providedIn: 'root' })
export class ProgressService {

  private readonly api = inject(ProgressGatewayService);

  private readonly progressSubject =
    new BehaviorSubject<ProgressGatewayControllerProgressRecordDto[]>([]);

  readonly progress$ = this.progressSubject.asObservable();

  // ===============================
  // LOAD PROGRESS
  // ===============================
  loadUserProgress(): Observable<ProgressGatewayControllerProgressRecordDto[]> {
    return this.api
      .postApiProgressUser({ withCredentials: true })
      .pipe(
        tap(progress => {
          this.progressSubject.next(progress);
        })
      );
  }

  refresh(): void {
    this.loadUserProgress().subscribe();
  }

  // ===============================
  // COMPLETE MODULE
  // ===============================
  completeModule(
    payload: GatewayContractsProgressModuleCompleteRequest
  ): Observable<void> {
    return this.api
      .postApiProgressCompleteModule(payload, { withCredentials: true })
      .pipe(
        tap(() => this.refresh())
      );
  }

  clear(): void {
    this.progressSubject.next([]);
  }
}


