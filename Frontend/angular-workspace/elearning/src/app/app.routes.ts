import { Routes } from '@angular/router';

export const routes: Routes = [

    {
        path : '',
        loadChildren: () =>
            import('./modules/auth/auth.routes').then(m => m.authRoutes)
    },
    {
        path : 'courses',
        loadChildren: () =>
            import('./modules/course/course.routes').then( m => m.courseRoutes)
    },
    {
        path : 'assessment',
        loadChildren: () =>
            import('./modules/assessment/assessment.routes').then(m => m.assessmentRoutes)
    },
    {
        path : 'user',
        loadChildren: () =>
            import('./modules/user/user.routes').then(m => m.userRoutes)
    }
];
