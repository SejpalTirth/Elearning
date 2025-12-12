import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from 'app/GatewayService/Auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent implements OnInit {

  loginMessage: string | null = null;
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['success'] === 'true') {
        this.loginMessage = 'Successfully Logged In!';
      }
    });
  }

  loginWithGoogle(): void {
    this.auth.loginWithGoogle();
  }

  loginWithMicrosoft(): void {
    this.auth.loginWithMicrosoft();
  }
}
