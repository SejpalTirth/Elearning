import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from './toast-service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.html',
  styleUrls: ['./toast.css']
})
export class ToastContainerComponent {
  constructor(public toastService: ToastService) {}
}