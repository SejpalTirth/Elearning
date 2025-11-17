import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-redirectpage',
  templateUrl: './redirectpage.html',
  styleUrls: ['./redirectpage.css']
})
export class RedirectPage implements OnInit {

  constructor(private router: Router, private route: ActivatedRoute) {}

  ngOnInit(): void {
    // Get token (if backend sends it as query param)
    const token = this.route.snapshot.queryParamMap.get('token');

    if (token) {
      // Save to local storage
      localStorage.setItem('access_token', token);
      console.log('Token saved:', token);
    }

    // Simulate a short delay before redirecting
    setTimeout(() => {
      this.router.navigate(['/dashboard']); // or wherever you want to go next
    }, 2000);
  }
}
