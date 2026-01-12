import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { NgApexchartsModule } from 'ng-apexcharts';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexTooltip,
  ApexStroke,
  ApexDataLabels,
  ApexTitleSubtitle
} from 'ng-apexcharts';
import { EntitiesService, EntityDetailsResponse, EntityTimelineResponse, MentioningArticlesResponse, ArticleDto, RelatedEntityResponse, EntityCoMentionDto } from '../../core/services/entities';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-entity-details',
  standalone: true,
  imports: [CommonModule, DatePipe, NgApexchartsModule],
  templateUrl: './entity-details.html',
  styleUrls: ['./entity-details.scss']
})
export class EntityDetailsComponent implements OnInit {
  entityId: string = '';
  entityDetails: EntityDetailsResponse | null = null;
  timelineData: EntityTimelineResponse | null = null;
  articles: ArticleDto[] = [];
  relatedEntities: EntityCoMentionDto[] = [];
  
  loading: boolean = false;
  error: string | null = null;
  
  // Chart options for timeline
  timelineChartOptions: any = {};
  timelineChartSeries: ApexAxisChartSeries = [];
  
  // Chart options for related entities
  relatedEntitiesChartOptions: any = {};
  relatedEntitiesChartSeries: ApexAxisChartSeries = [];

  constructor(
    private route: ActivatedRoute,
    private entitiesService: EntitiesService
  ) {}

  ngOnInit(): void {
    this.entityId = this.route.snapshot.paramMap.get('id') || '';
    if (this.entityId) {
      this.loadEntityDetails();
    }
  }

  loadEntityDetails(): void {
    this.loading = true;
    this.error = null;
    
    // Get dates for the last 30 days
    const today = new Date();
    const fromDate = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);
    fromDate.setHours(0, 0, 0, 0);
    const toDate = new Date(today);
    toDate.setHours(23, 59, 59, 999);
    
    // Load all data in parallel
    this.entitiesService.getEntityDetails(
      this.entityId,
      fromDate.toISOString().slice(0, 10),
      toDate.toISOString().slice(0, 10)
    ).pipe(
      catchError(err => {
        console.error('Failed to load entity details', err);
        this.error = 'Failed to load entity details.';
        return of(null as EntityDetailsResponse | null);
      })
    ).subscribe(details => {
      this.entityDetails = details;
      this.loading = false;
    });
    
    this.entitiesService.getEntityTimeline(
      this.entityId,
      fromDate.toISOString().slice(0, 10),
      toDate.toISOString().slice(0, 10)
    ).pipe(
      catchError(err => {
        console.error('Failed to load entity timeline', err);
        return of(null as EntityTimelineResponse | null);
      })
    ).subscribe(timeline => {
      this.timelineData = timeline;
      this.updateTimelineChart();
    });
    
    this.entitiesService.getEntityArticles(
      this.entityId,
      fromDate.toISOString().slice(0, 10),
      toDate.toISOString().slice(0, 10)
    ).pipe(
      catchError(err => {
        console.error('Failed to load entity articles', err);
        return of({ articles: [] } as MentioningArticlesResponse);
      })
    ).subscribe(response => {
      this.articles = response.articles || [];
    });
    
    this.entitiesService.getRelatedEntities(
      this.entityId,
      fromDate.toISOString().slice(0, 10),
      toDate.toISOString().slice(0, 10)
    ).pipe(
      catchError(err => {
        console.error('Failed to load related entities', err);
        return of({ coMentionedEntities: [] } as RelatedEntityResponse);
      })
    ).subscribe(response => {
      this.relatedEntities = response.coMentionedEntities || [];
      this.updateRelatedEntitiesChart();
    });
  }

  updateTimelineChart(): void {
    if (!this.timelineData || !this.timelineData.timeline) {
      return;
    }
    
    const dates = this.timelineData.timeline.map(point => point.date);
    const counts = this.timelineData.timeline.map(point => point.count);
    
    this.timelineChartSeries = [{
      name: 'Mentions',
      data: counts
    }];
    
    this.timelineChartOptions = {
      chart: {
        type: 'area',
        height: 350,
        zoom: {
          enabled: false
        }
      },
      xaxis: {
        categories: dates,
        title: {
          text: 'Date'
        }
      },
      yaxis: {
        title: {
          text: 'Mentions'
        }
      },
      tooltip: {
        shared: false,
        y: {
          formatter: (value: number) => value.toString()
        }
      },
      stroke: {
        curve: 'smooth'
      },
      title: {
        text: `Mentions Timeline for ${this.entityDetails?.name || this.entityId}`,
        align: 'left'
      }
    };
  }

  updateRelatedEntitiesChart(): void {
    if (!this.relatedEntities || this.relatedEntities.length === 0) {
      return;
    }
    
    const names = this.relatedEntities.map(entity => entity.name || 'Unknown');
    const counts = this.relatedEntities.map(entity => entity.coMentionCount);
    
    this.relatedEntitiesChartSeries = [{
      name: 'Co-mentions',
      data: counts
    }];
    
    this.relatedEntitiesChartOptions = {
      chart: {
        type: 'bar',
        height: 350
      },
      plotOptions: {
        bar: {
          horizontal: true,
        }
      },
      xaxis: {
        categories: names,
        title: {
          text: 'Co-mention Count'
        }
      },
      yaxis: {
        title: {
          text: 'Entity'
        }
      },
      tooltip: {
        y: {
          formatter: (value: number) => value.toString()
        }
      },
      title: {
        text: `Related Entities for ${this.entityDetails?.name || this.entityId}`,
        align: 'left'
      }
    };
  }
}