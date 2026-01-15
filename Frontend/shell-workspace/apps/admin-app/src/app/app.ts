import { Component, signal } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';

import { AuthStateService } from '@frontend/auth';
import { ADMIN_MENU } from './admin.menu.config';
import {
  Loading,
  SharedHeaderComponent,
  ToastContainerComponent
} from '@frontend/ui';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    SharedHeaderComponent,
    ToastContainerComponent,
    Loading
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {

  protected readonly title = signal('admin-app');

  showHeader = signal(true);

  menu = ADMIN_MENU;

  constructor(
    authState: AuthStateService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    authState.initialize();

    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        let current = this.route.firstChild;
        while (current?.firstChild) {
          current = current.firstChild;
        }

        const layout = current?.snapshot.data?.['layout'];
        this.showHeader.set(layout !== 'denied');
      });
  }
}
