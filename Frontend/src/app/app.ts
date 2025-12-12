import { Component, inject } from '@angular/core';
import { Router, NavigationEnd, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

import { HeaderComponent } from './GatewayService/header/header';
import { SharedHeaderComponent } from './shared/header/header';
import { ToastContainerComponent } from './shared/toast/toast';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    HeaderComponent,
    SharedHeaderComponent,
    ToastContainerComponent
  ],
  templateUrl: './app.html'
})
export class App {

  private readonly _router = inject(Router);

  currentUrl = '';

  constructor() {
    this._router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.currentUrl = event.url;
      }
    });
  }

  isLoginPage(): boolean {
    return this.currentUrl === '/' ||
           this.currentUrl.startsWith('/gateway/auth/callback');
  }
}
