import { Component, OnInit } from '@angular/core';
import { TrendingService } from '../services/trending.service';

@Component({
  selector: 'app-trending-mentions',
  standalone: true,
  templateUrl: './trending-mentions.component.html',
  styleUrls: ['./trending-mentions.component.css']
})
export class TrendingMentionsComponent implements OnInit {
  trending: any[] = [];
  loading = false;
  error = '';

  constructor(private trendingService: TrendingService) {}

  ngOnInit() {
    this.fetchTrending();
  }

  fetchTrending() {
    this.loading = true;
    this.error = '';
    this.trendingService.getTrending()
      .subscribe({
        next: (data) => {
          this.trending = data;
          this.loading = false;
        },
        error: (err) => {
          this.error = 'Failed to load trending mentions.';
          this.loading = false;
        }
      });
  }
}
