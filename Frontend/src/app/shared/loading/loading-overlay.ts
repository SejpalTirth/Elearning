import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading-overlay.html',
  styleUrls: ['./loading-overlay.css']
})
export class LoadingOverlay {
  visible = false;

  show(): void { this.visible = true; }
  hide(): void { this.visible = false; }
}
