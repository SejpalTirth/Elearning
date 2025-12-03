import { Component } from '@angular/core';
import { Router, NavigationEnd, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

import { HeaderComponent } from './GatewayService/header/header';
import { SharedHeaderComponent } from './shared/header/header';
import { ToastContainerComponent } from "./shared/toast/toast";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, // REQUIRED for *ngIf
    RouterOutlet,
    HeaderComponent,
    SharedHeaderComponent,
    ToastContainerComponent
],
  templateUrl: './app.html'
})
export class App {

  currentUrl = '';

  constructor(private router: Router) {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.currentUrl = event.url;
      }
    });
  }

  isLoginPage() {
    return this.currentUrl === '/' ||
           this.currentUrl.startsWith('/gateway/auth/callback');
  }

}
