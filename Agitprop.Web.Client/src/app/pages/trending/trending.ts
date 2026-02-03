import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgApexchartsModule } from 'ng-apexcharts';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexStroke,
  ApexXAxis,
  ApexTooltip,
  ApexLegend,
  ApexNonAxisChartSeries,
  ApexDataLabels,
  ApexTitleSubtitle
} from 'ng-apexcharts';
import { catchError, of } from 'rxjs';
import { TrendingService, TrendingResponse } from '../../core/services/trending';
import { EntitiesService, EntityDetailsDto, ArticleDto, MentioningArticlesResponse } from '../../core/services/entities';

export interface SparklineChartOptions {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  stroke: ApexStroke;
  tooltip: ApexTooltip;
  xaxis: ApexXAxis;
  colors: string[];
}

export interface PieChartOptions {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  legend: ApexLegend;
  labels: any;
  dataLabels: ApexDataLabels;
  title: ApexTitleSubtitle;
}

@Component({
  selector: 'app-trending',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, NgApexchartsModule],
  templateUrl: './trending.html',
  styleUrls: ['./trending.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class TrendingComponent implements OnInit {
  fromDate: Date;
  toDate: Date;
  dateRange: { start: Date; end: Date };

  trending: (EntityDetailsDto & {
    chartOptions: SparklineChartOptions;
    selected: boolean;
    expanded: boolean;
    articles?: ArticleDto[];
    domainPieData?: { [domain: string]: number };
    pieChartOptions?: PieChartOptions;
  })[] = [];
  loading = true;

  bigChartSeries: ApexAxisChartSeries = [];
  bigChartOptions: any = {};

  constructor(private trendingService: TrendingService, private entityService: EntitiesService) {
    const today = new Date();
    this.toDate = today;
    this.fromDate = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000); // elmúlt 7 nap
    this.dateRange = { start: this.fromDate, end: this.toDate };
  }

  ngOnInit(): void {
    this.loadTrending();
  }

  onStartChange(value: string) {
    try {
      this.dateRange.start = new Date(value);
    } catch { }
  }

  onEndChange(value: string) {
    try {
      this.dateRange.end = new Date(value);
    } catch { }
  }

  loadTrending(): void {
    this.loading = true;
    // Normalize dates to start of day for consistent API calls
    const fromDate = new Date(this.fromDate);
    fromDate.setHours(0, 0, 0, 0);
    const toDate = new Date(this.toDate);
    toDate.setHours(23, 59, 59, 999);
    
    this.trendingService
      .getTrending(fromDate.toISOString().slice(0, 10), toDate.toISOString().slice(0, 10))
      .pipe(
        catchError(err => {
          console.error('Trending load failed', err);
          return of({ trending: [] } as TrendingResponse);
        })
      )
      .subscribe((data: TrendingResponse) => {
        // create a full list of dates in the selected range so every entity has a value for each day
        console.log('Load trending data for date range:', this.fromDate, 'to', this.toDate);
        const categories = this.buildDateCategories(this.fromDate, this.toDate);

        this.trending = data.trending.map(entity => {
          const countsMap = entity.mentionsCountByDate || {};
          // fill missing dates with 0 so series lengths match the categories
          const seriesData = categories.map((d: string) => countsMap[d] || 0);
          const color = seriesData[seriesData.length - 1] > seriesData[0] ? '#00C853' : '#D32F2F';

          const chartOptions: SparklineChartOptions = {
            series: [{ data: seriesData }],
            colors: [color],
            chart: {
              type: 'line',
              sparkline: { enabled: true },
              height: 50,
              width: 150,
              zoom: { enabled: false }
            },
            stroke: { width: 2 },
            tooltip: { enabled: false },
            xaxis: { categories: categories }
          };

          return { ...entity, chartOptions, selected: false, expanded: false };
        });

        this.trending.slice(0, 5).forEach(e => e.selected = true);
        this.updateBigChart();
        this.loading = false;
      });
  }

  onDateRangeChange(event: any) {
    // az event.value tartalmazza a start és end dátumokat
    if (event && event.value) {
      this.dateRange = { start: event.value.start, end: event.value.end };
    }
  }

  applyDateRange() {
    this.fromDate = this.dateRange.start;
    this.toDate = this.dateRange.end;
    this.loadTrending();
  }

  toggleEntitySelection(entity: EntityDetailsDto & any) {
    entity.selected = !entity.selected;
    this.updateBigChart();
  }

  toggleExpand(entity: EntityDetailsDto & any) {
    entity.expanded = !entity.expanded;
    if (entity.expanded && !entity.articles) {
      this.entityService
        .getEntityArticles(entity.id!, this.fromDate.toISOString().slice(0, 10), this.toDate.toISOString().slice(0, 10))
        .pipe(catchError(() => of({ articles: [] } as MentioningArticlesResponse)))
        .subscribe(res => {
          entity.articles = res.articles || [];
          entity.domainPieData = {};
          entity.articles.forEach((a: { articleUrl: any; }) => {
            try {
              const url = new URL(a.articleUrl || '');
              entity.domainPieData![url.hostname] = (entity.domainPieData![url.hostname] || 0) + 1;
            } catch { }
          });

          const pieSeries = Object.values(entity.domainPieData!);
          const pieLabels = Object.keys(entity.domainPieData!);

          entity.pieChartOptions = {
            series: pieSeries,
            labels: pieLabels,
            chart: {
              type: 'pie',
              width: 300,
              height: 300
            },
            legend: {
              show: true,
              position: 'bottom',

            },
            dataLabels: {
              enabled: false
            },
            title: {
              text: 'Distribution',
              align: 'center'
            }
          };
        });
    }
  }

  private updateBigChart() {
    const categories: string[] = this.buildDateCategories(this.fromDate, this.toDate);
    const selectedEntities = this.trending.filter(e => e.selected);

    this.bigChartSeries = selectedEntities.map(e => {
      const countsMap = e.mentionsCountByDate || {};
      const data = categories.map((d: string) => countsMap[d] || 0);
      return { name: e.name, data };
    });

    this.bigChartOptions = {
      chart: {
        type: 'line',
        height: 300,
        zoom: { enabled: false }
      },
      stroke: { width: 2 },
      xaxis: {
        categories,
        type: 'category'
      },
      tooltip: { shared: true },
      legend: { position: 'top' }
    };
  }

  private buildDateCategories(start: Date, end: Date): string[] {
    const list: string[] = [];
    if (!start || !end) return list;
    
    // Create new dates and set to midnight UTC
    const startDate = new Date(Date.UTC(start.getFullYear(), start.getMonth(), start.getDate()));
    const endDate = new Date(Date.UTC(end.getFullYear(), end.getMonth(), end.getDate()));
    
    // Iterate through dates inclusively (using <=)
    const cur = new Date(startDate);
    while (cur <= endDate) {
      list.push(cur.toISOString().slice(0, 10));
      cur.setUTCDate(cur.getUTCDate() + 1);
    }
    
    return list;
  }
}
