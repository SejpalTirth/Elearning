import { Component, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { AuthStateService } from '@frontend/auth';
import { Instructor_MENU } from './instructor.menu.config';
import { Loading, SharedHeaderComponent, ToastContainerComponent } from '@frontend/ui';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
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
  protected readonly title = signal('instructor-app');

  showHeader = signal(true);

  menu = Instructor_MENU;
  constructor(
    authstate : AuthStateService,
    private router : Router,
    private route : ActivatedRoute 
  )
  {
    authstate.initialize();

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
