import { Routes } from '@angular/router';
import { TrendingComponent } from './pages/trending/trending';
import { EntitiesComponent } from './pages/entities/entities';
import { EntityDetailsComponent } from './pages/entity-details/entity-details';

export const routes: Routes = [
    { path: 'trending', component: TrendingComponent },
    { path: 'entities', component: EntitiesComponent },
    { path: 'entities/:id', component: EntityDetailsComponent }
];
