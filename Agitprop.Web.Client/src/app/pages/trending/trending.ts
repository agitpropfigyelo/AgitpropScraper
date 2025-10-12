import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TrendingService, TrendingEntity, TrendingResponse } from '../../core/services/trending';
import {
  NgApexchartsModule,
  ApexAxisChartSeries,
  ApexChart,
  ApexStroke,
  ApexXAxis,
  ApexTooltip
} from 'ng-apexcharts';
import { catchError, of } from 'rxjs';
import { DateRangePicker } from "../../shared/date-range-picker/date-range-picker";

export interface SparklineChartOptions {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  stroke: ApexStroke;
  tooltip: ApexTooltip;
  xaxis: ApexXAxis;
  colors: string[];
}

@Component({
  selector: 'app-trending',
  standalone: true,
  imports: [CommonModule, NgApexchartsModule, DateRangePicker],
  templateUrl: './trending.html',
  styleUrls: ['./trending.scss']
})
export class TrendingComponent implements OnInit {
  trending: (TrendingEntity & { chartOptions: SparklineChartOptions })[] = [];
  loading = true;

  private readonly defaultChartOptions: SparklineChartOptions = {
    series: [{ data: [] }],
    chart: { type: 'line', sparkline: { enabled: true } },
    stroke: { width: 2 },
    colors: ['#ccc'],
    tooltip: { enabled: false },
    xaxis: { categories: [] }
  };
  fromDate: any;
  toDate: any;

  constructor(private trendingService: TrendingService) { }

  ngOnInit(): void {
    const today = new Date();
    this.toDate = today;

    const sevenDaysAgo = new Date();
    sevenDaysAgo.setDate(today.getDate() - 7);
    this.fromDate = sevenDaysAgo;
    
    this.loadTrending();
  }

  loadTrending(): void {
    this.loading = true;

    this.trendingService.getTrending(
      this.fromDate.toISOString().slice(0, 10),
      this.toDate.toISOString().slice(0, 10)
    )
      .pipe(
        catchError(err => {
          console.error('Failed to load trending data', err);
          return of({ trending: [] } as TrendingResponse);
        })
      )
      .subscribe(data => {
        this.trending = data.trending.map(entity => {
          const sorted = Object.entries(entity.mentionsCountByDate)
            .sort(([a], [b]) => new Date(a).getTime() - new Date(b).getTime());

          const seriesData = sorted.map(([, count]) => count);
          const color = seriesData[seriesData.length - 1] > seriesData[0] ? '#00C853' : '#D32F2F';

          return {
            ...entity,
            chartOptions: {
              ...this.defaultChartOptions,
              series: [{ data: seriesData }],
              colors: [color],
              xaxis: { categories: sorted.map(([d]) => d) },
              chart: { ...this.defaultChartOptions.chart, height: 50, width: 150 }
            }
          };
        });

        this.loading = false;
      });
  }


  onDateRangeChange(event: { from: Date; to: Date }): void {
    this.fromDate = event.from;
    this.toDate = event.to;
    this.loadTrending();
  }

}
