import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from './LoadingService';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading-overlay.html',
  styleUrls: ['./loading-overlay.css']
})
export class LoadingOverlay {

  constructor(public loading: LoadingService) {}
}
