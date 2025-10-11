import { Component, OnInit } from '@angular/core';
import { TrendingService, TrendingEntity } from '../../core/services/trending';
import {CommonModule} from "@angular/common";

@Component({
  selector: 'app-trending',
  standalone: true,
  templateUrl: './trending.html',
  styleUrl: './trending.scss',
  imports: [CommonModule]
})
export class Trending implements OnInit {
  trending: TrendingEntity[] = [];
  loading = true;

  constructor(private trendingService: TrendingService) {}

  ngOnInit() {
    const today = new Date();
    const weekAgo = new Date();
    weekAgo.setDate(today.getDate() - 7);

    const from = weekAgo.toISOString().split('T')[0];
    const to = today.toISOString().split('T')[0];

    this.trendingService.getTrending(from, to).subscribe({
      next: (response) => {
        this.trending = response.trending;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load trending data', err);
        this.loading = false;
      }
    });
  }
}
