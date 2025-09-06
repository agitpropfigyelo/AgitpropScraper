import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class EntityService {
  constructor(private http: HttpClient) {}

  searchEntities(term: string): Observable<any[]> {
    // TODO: Replace with actual API endpoint and params
    return this.http.get<any>(`/api/entity?search=${encodeURIComponent(term)}&page=1&pageSize=25`)
      .pipe(
        map((res: any) => res || []),
        catchError(() => of([]))
      );
  }

  getEntityTimeline(entityId: string): Observable<any[]> {
    // TODO: Replace with actual API endpoint and params
    return this.http.get<any>(`/api/entity/${entityId}/timeline`)
      .pipe(
        map((res: any) => res.timeline || []),
        catchError(() => of([]))
      );
  }

  getAssociatedEntities(entityId: string): Observable<any[]> {
    // TODO: Replace with actual API endpoint and params
    return this.http.get<any>(`/api/entity/${entityId}/associated`)
      .pipe(
        map((res: any) => res.associated?.slice(0, 25) || []),
        catchError(() => of([]))
      );
  }
}
