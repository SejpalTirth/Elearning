import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'loading-overlay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading-overlay.html',
  styleUrls: ['./loading-overlay.css']
})
export class LoadingOverlay {
  visible = false;

  show() { this.visible = true; }
  hide() { this.visible = false; }
}
