import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from './loading-service';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading.html',
  styleUrls: ['./loading.css']
})
export class Loading {

  constructor(public loading: LoadingService) {}
}