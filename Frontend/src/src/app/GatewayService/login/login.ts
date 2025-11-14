import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common'; // Required for *ngIf

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent implements OnInit {
  loginMessage: string | null = null;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['success'] === 'true') {
        this.loginMessage = 'Successfully Logged In!';
      }
    });
  }

  loginWithGoogle(): void {
    window.location.href = 'https://localhost:7249/api/GatewayAuth/google-login';
  }

  loginWithMicrosoft(): void {
    window.location.href = 'https://localhost:7249/api/GatewayAuth/microsoft-login';
  }
}
