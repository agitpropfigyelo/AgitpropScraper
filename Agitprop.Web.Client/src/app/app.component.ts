import { Component, Injectable } from '@angular/core';

import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { WeatherForecasts } from '../types/weatherForecast';
import { TrendingMentionsComponent } from './components/trending-mentions.component';
import { EntitySearchComponent } from './components/entity-search.component';
import { EntityTimelineComponent } from './components/entity-timeline.component';
import { AssociatedEntitiesComponent } from './components/associated-entities.component';

@Injectable()
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    TrendingMentionsComponent,
    EntitySearchComponent,
    EntityTimelineComponent,
    AssociatedEntitiesComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {

  title = 'weather';
  forecasts: WeatherForecasts = [];
  selectedEntity: any = null;

  constructor(private http: HttpClient) {
    http.get<WeatherForecasts>('api/weatherforecast').subscribe({
      next: result => this.forecasts = result,
      error: console.error
    });
  }
}
