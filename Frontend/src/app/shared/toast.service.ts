import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ToastService {

  toasts: { type: 'success' | 'error', message: string }[] = [];

  showSuccess(message: string) {
    this.toasts.push({ type: 'success', message });
    this.autoRemove();
  }

  showError(message: string) {
    this.toasts.push({ type: 'error', message });
    this.autoRemove();
  }

  private autoRemove() {
    setTimeout(() => {
      this.toasts.shift();
    }, 3000);
  }
}
