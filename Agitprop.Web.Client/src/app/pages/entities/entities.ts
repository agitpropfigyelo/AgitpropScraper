import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { EntitiesService, EntityDto, PaginatedEntitiesResponse } from '../../core/services/entities';
import { debounceTime, distinctUntilChanged, switchMap, catchError, of } from 'rxjs';

@Component({
  selector: 'app-entities',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, RouterModule],
  templateUrl: './entities.html',
  styleUrls: ['./entities.scss']
})
export class EntitiesComponent implements OnInit {
  searchQuery: string = '';
  fromDate: Date;
  toDate: Date;
  dateRange: { start: Date; end: Date };
  
  entities: EntityDto[] = [];
  filteredEntities: EntityDto[] = [];
  loading: boolean = false;
  error: string | null = null;
  
  // Pagination
  currentPage: number = 1;
  pageSize: number = 20;
  totalPages: number = 1;
  
  // Autocomplete
  autocompleteResults: EntityDto[] = [];
  showAutocomplete: boolean = false;

  constructor(private entitiesService: EntitiesService) {
    const today = new Date();
    this.toDate = today;
    this.fromDate = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000); // Last 7 days
    this.dateRange = { start: this.fromDate, end: this.toDate };
  }

  ngOnInit(): void {
    this.loadEntities();
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

  applyDateRange(): void {
    this.currentPage = 1;
    this.loadEntities();
  }

  loadEntities(): void {
    this.loading = true;
    this.error = null;
    
    // Normalize dates to start of day for consistent API calls
    const fromDate = new Date(this.fromDate);
    fromDate.setHours(0, 0, 0, 0);
    const toDate = new Date(this.toDate);
    toDate.setHours(23, 59, 59, 999);
    
    this.entitiesService
      .getEntities(
        fromDate.toISOString().slice(0, 10),
        toDate.toISOString().slice(0, 10),
        this.currentPage,
        this.pageSize,
        this.searchQuery
      )
      .pipe(
        catchError(err => {
          console.error('Failed to load entities', err);
          this.error = 'Failed to load entities. Please try again.';
          return of({ entities: [], page: 1, pageSize: this.pageSize } as PaginatedEntitiesResponse);
        })
      )
      .subscribe(response => {
        this.entities = response.entities || [];
        this.filteredEntities = this.entities;
        this.totalPages = Math.ceil((response.entities?.length || 0) / this.pageSize);
        this.loading = false;
      });
  }

  onSearchInput(value: string): void {
    this.searchQuery = value;
    
    if (this.searchQuery.length > 2) {
      // Fetch all entities for autocomplete
      const fromDate = new Date(this.fromDate);
      fromDate.setHours(0, 0, 0, 0);
      const toDate = new Date(this.toDate);
      toDate.setHours(23, 59, 59, 999);
      
      this.entitiesService.getAllEntities(
        fromDate.toISOString().slice(0, 10),
        toDate.toISOString().slice(0, 10)
      ).subscribe(allEntities => {
        this.autocompleteResults = allEntities.filter(entity =>
          entity.name.toLowerCase().includes(this.searchQuery.toLowerCase())
        );
        this.showAutocomplete = true;
      });
    } else {
      this.showAutocomplete = false;
    }
  }

  selectAutocompleteResult(entity: EntityDto): void {
    this.searchQuery = entity.name;
    this.showAutocomplete = false;
    this.currentPage = 1;
    this.loadEntities();
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadEntities();
    }
  }

  prevPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadEntities();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadEntities();
    }
  }
}