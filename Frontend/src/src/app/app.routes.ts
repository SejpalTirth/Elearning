import { Routes } from '@angular/router';
import { LoginComponent } from './GatewayService/login/login';

export const routes: Routes = [
  { path: '', component: LoginComponent },
  { path: '**', redirectTo: '' }
];
