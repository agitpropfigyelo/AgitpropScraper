import { Component, Input, OnChanges } from '@angular/core';
import { EntityService } from '../services/entity.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-entity-timeline',
  standalone: true,
  templateUrl: './entity-timeline.component.html',
  styleUrls: ['./entity-timeline.component.css'],
  imports: [CommonModule]
})
export class EntityTimelineComponent implements OnChanges {
  @Input() entity: any;
  timeline: any[] = [];
  loading = false;
  error = '';

  constructor(private entityService: EntityService) {}

  ngOnChanges() {
    if (this.entity) {
      this.fetchTimeline();
    }
  }

  fetchTimeline() {
    this.loading = true;
    this.error = '';
    this.entityService.getEntityTimeline(this.entity.id)
      .subscribe({
        next: (data) => {
          this.timeline = data;
          this.loading = false;
        },
        error: (err) => {
          this.error = 'Failed to load timeline.';
          this.loading = false;
        }
      });
  }
}
