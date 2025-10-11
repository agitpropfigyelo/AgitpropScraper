import { Routes } from '@angular/router';
import { Trending } from './pages/trending/trending';

export const routes: Routes = [
    { path: '', redirectTo: 'trending', pathMatch: 'full' },
    { path: 'trending', component: Trending }
];
