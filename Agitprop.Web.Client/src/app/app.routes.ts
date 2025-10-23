import { Routes } from '@angular/router';
import { TrendingComponent } from './pages/trending/trending';

export const routes: Routes = [
    { path: '', redirectTo: 'trending', pathMatch: 'full' },
    { path: 'trending', component: TrendingComponent }
];
