import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from "./header/header";
import { ToastContainerComponent } from '@frontend/ui'

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    Header,
    ToastContainerComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('shell-app');
}
