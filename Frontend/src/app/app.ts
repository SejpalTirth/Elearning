import { Component, inject } from '@angular/core';
import { Router, NavigationEnd, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

import { HeaderComponent } from './GatewayService/header/header';
import { SharedHeaderComponent } from './shared/header/header';
import { ToastContainerComponent } from './shared/toast/toast';
import { AuthStateService } from './GatewayService/Auth/auth-state.service';
import { LoadingOverlay } from './shared/loading/loading-overlay';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    HeaderComponent,
    SharedHeaderComponent,
    ToastContainerComponent,
    LoadingOverlay
],
  templateUrl: './app.html'
})
export class App {

  private readonly _authState = inject(AuthStateService);
  currentUrl = '';

  constructor(private router: Router) {

    // AUTH MUST INIT IMMEDIATELY
    this._authState.initialize();

    // URL tracking only
    this.router.events.subscribe(event => {
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
