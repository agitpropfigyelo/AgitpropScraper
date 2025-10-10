import { Component, Input, OnChanges } from '@angular/core';
import { EntityService } from '../services/entity.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-associated-entities',
  standalone: true,
  templateUrl: './associated-entities.component.html',
  styleUrls: ['./associated-entities.component.css'],
  imports: [CommonModule]
})
export class AssociatedEntitiesComponent implements OnChanges {
  @Input() entity: any;
  associated: any[] = [];
  loading = false;
  error = '';

  constructor(private entityService: EntityService) {}

  ngOnChanges() {
    if (this.entity) {
      this.fetchAssociated();
    }
  }

  fetchAssociated() {
    this.loading = true;
    this.error = '';
    this.entityService.getAssociatedEntities(this.entity.id)
      .subscribe({
        next: (data) => {
          this.associated = data;
          this.loading = false;
        },
        error: (err) => {
          this.error = 'Failed to load associated entities.';
          this.loading = false;
        }
      });
  }
}
