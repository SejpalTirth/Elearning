import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class DenialLockGuard implements CanDeactivate<unknown> {
  canDeactivate(): boolean {
    return false;
  }
}
