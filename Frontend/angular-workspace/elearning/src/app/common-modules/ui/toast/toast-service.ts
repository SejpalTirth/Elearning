import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ToastService {

  toasts: { type: 'success' | 'error' | 'info', message: string }[] = [];

  showSuccess(message: string): void {
    this.toasts.push({ type: 'success', message });
    this.autoRemove();
  }

  showError(message: string): void {
    this.toasts.push({ type: 'error', message });
    this.autoRemove();
  }

  showInfo(message: string): void {
    this.toasts.push({ type: 'info', message });
    this.autoRemove();
  }

  private autoRemove():void {
    setTimeout(() => {
      this.toasts.shift();
    }, 3000);
  }
}