import { Routes } from '@angular/router';
import { Home } from '../features/home/home';
import { NotFound } from '../shared/components/errors/not-found/not-found';
import { Play } from '../features/play/play';

export const routes: Routes = [
    { path: '', component: Home },
    { path: 'play', component: Play },
    { path: "account", loadChildren: () => import('../features/account/routes').then(r => r.accountRoutes)  },
    { path: "not-found", component: NotFound },
    { path: "**", component: NotFound, pathMatch: 'full' },
];
