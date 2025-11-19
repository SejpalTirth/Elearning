import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-complete-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './complete-profile.html',
  styleUrls: ['./complete-profile.css'],
})
export class CompleteProfileComponent implements OnInit {

  userId: string | null = null;
  name: string = "";
  roleId: number = 3; // Default: Student
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userId = this.route.snapshot.queryParamMap.get('userId');

    if (!this.userId) {
      alert("User ID missing. Please login again.");
      this.router.navigate(['/login']);
      return;
    }
  }

  saveProfile() {
    if (!this.name.trim()) {
      alert("Please enter your name.");
      return;
    }

    this.loading = true;

    const payload = {
      userId: this.userId,
      name: this.name,
      roleId: this.roleId
    };

    this.http.post("https://localhost:7130/api/users/complete-profile", payload)
      .subscribe({
        next: () => {
          alert("Profile completed successfully! Please login again.");
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.loading = false;
          alert(err.error?.message || "Something went wrong.");
        }
      });
  }
}
