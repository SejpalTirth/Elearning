import { Routes } from "@angular/router";

export const userRoutes : Routes = [
    {
        path : 'manage-users',
        loadComponent: () =>
            import('./manage-user/manage-user').then(m => m.ManageUsersComponent)
    }
]