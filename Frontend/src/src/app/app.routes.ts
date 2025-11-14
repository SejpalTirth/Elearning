import { Routes } from '@angular/router';
import { RedirectPage } from './GatewayService/RedirectComponent/redirectpage';
import { LoginComponent } from './GatewayService/login/login';
import { HeaderComponent } from './GatewayService/header/header';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'redirect', component: RedirectPage },
  { path: 'header', component: HeaderComponent },
];
