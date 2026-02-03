import { Routes } from "@angular/router";

export const authRoutes : Routes = [
    {
        path:'',
        loadComponent: () =>
            import('./login/login').then(m => m.LoginComponent)
    },
    {
        path:'auth/callback',
        loadComponent: () =>
            import('./auth-callback/auth-callback').then(m => m.AuthCallback)
    },
    {
        path:'sign-up',
        loadComponent: () =>
            import('./sign-up/sign-up').then(m => m.SignUpComponent)
    },
    {
        path:'home',
        loadComponent: () =>
            import('./home/home').then(m => m.Home)
    }
];