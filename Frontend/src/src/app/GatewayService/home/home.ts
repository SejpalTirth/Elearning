import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home implements OnInit {

  userName: string | null = null;
  userRole: string | null = null;

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.loadUserInfo();
  }

  loadUserInfo() {
    const token = localStorage.getItem('token');
    if (!token) {
      this.router.navigate(['/']);
      return;
    }

    // Decode JWT payload
    const payload = JSON.parse(atob(token.split('.')[1]));

    // Extract name, role
    this.userName = payload['name'] || 'User';
    this.userRole = payload['role'] || 'Unknown';
  }

  logout() {
    const refresh = localStorage.getItem('refresh');

    fetch("https://localhost:7249/api/GatewayAuth/logout", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(refresh)
    }).finally(() => {
      localStorage.removeItem('token');
      localStorage.removeItem('refresh');

      this.router.navigate(['/']);
    });
  }

}
