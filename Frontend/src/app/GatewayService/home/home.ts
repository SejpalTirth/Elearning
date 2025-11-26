import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home implements OnInit {

  userName: string | null = null;
  role: string | null = null;

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

    const payload = JSON.parse(atob(token.split('.')[1]));

    this.userName = payload['name'] || 'User';
    this.role = payload['role'] || null;
  }

  goToCourses() {
    this.router.navigate(['/courses']);
  }
}
