import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './GatewayService/header/header';
import { LoginComponent } from './GatewayService/login/login';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, LoginComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {}
