import { Component, EventEmitter, Output } from '@angular/core';
import { EntityService } from '../services/entity.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-entity-search',
  standalone: true,
  templateUrl: './entity-search.component.html',
  styleUrls: ['./entity-search.component.css'],
  imports: [CommonModule, FormsModule]
})
export class EntitySearchComponent {
  entities: any[] = [];
  searchTerm = '';
  loading = false;
  error = '';
  @Output() entitySelected = new EventEmitter<any>();

  constructor(private entityService: EntityService) {}

  onSearch(term: string) {
    this.loading = true;
    this.error = '';
    this.entityService.searchEntities(term)
      .subscribe({
        next: (data) => {
          this.entities = data;
          this.loading = false;
        },
        error: (err) => {
          this.error = 'Failed to load entities.';
          this.loading = false;
        }
      });
  }

  selectEntity(entity: any) {
    this.entitySelected.emit(entity);
  }
}
