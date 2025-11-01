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
    this.trendingService
      .getTrending(this.fromDate.toISOString().slice(0, 10), this.toDate.toISOString().slice(0, 10))
      .pipe(
        catchError(err => {
          console.error('Trending load failed', err);
          return of({ trending: [] } as TrendingResponse);
        })
      )
      .subscribe((data: TrendingResponse) => {
        this.trending = data.trending.map(entity => {
          const sorted = Object.entries(entity.mentionsCountByDate || {}).sort(
            ([a], [b]) => new Date(a).getTime() - new Date(b).getTime()
          );
          console.log('Sorted:',sorted);
          const seriesData = sorted.map(([_, count]) => count);
          console.log('SeriesData',seriesData);
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
            xaxis: { categories: sorted.map(([d]) => d) }
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
    const selectedEntities = this.trending.filter(e => e.selected);
    this.bigChartSeries = selectedEntities.map(e => {
      const sorted = Object.entries(e.mentionsCountByDate || {}).sort(
        ([a], [b]) => new Date(a).getTime() - new Date(b).getTime()
      );
      return { name: e.name, data: sorted.map(([_, count]) => count) };
    });

    this.bigChartOptions = {
      chart: { 
        type: 'line', 
        height: 300,
        zoom: { enabled: false }
      },
      stroke: { width: 2 },
      xaxis: { categories: selectedEntities.length > 0 ? Object.keys(selectedEntities[0].mentionsCountByDate!) : [] },
      tooltip: { shared: true },
      legend: { position: 'top' }
    };
  }
}
