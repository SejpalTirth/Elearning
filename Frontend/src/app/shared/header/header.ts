import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'shared-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class SharedHeaderComponent implements OnInit {

  userName: string | null = null;
  role: string | null = null;
  loggedIn = false;

  dropdownOpen: boolean = false;

  constructor(private router: Router) {}

  ngOnInit(): void {  // <-- THIS WAS NOT WORKING BEFORE
    const token = localStorage.getItem('token');

    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.userName = payload['name'] || payload['email'] || 'User';
        this.role = payload['role'] || null;
        this.loggedIn = true;
      } catch {
        this.loggedIn = false;
      }
    }
  }

  toggleDropdown() {
    this.dropdownOpen = !this.dropdownOpen;
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/']);
  }

  toggleTheme() {
  document.body.classList.toggle('dark-theme');
  }

}
