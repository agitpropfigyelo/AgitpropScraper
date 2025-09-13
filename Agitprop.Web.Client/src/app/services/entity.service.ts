import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map, retry } from 'rxjs/operators';
import { timeout } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class EntityService {
  constructor(private http: HttpClient) {}

  private readonly retryCount = 3;

  searchEntities(term: string): Observable<any[]> {
    // TODO: Replace with actual API endpoint and params
    return this.http.get<any>(`api/entity?search=${encodeURIComponent(term)}&page=1&pageSize=25`)
      .pipe(
        timeout(10000),
        retry(this.retryCount),
        map((res: any) => res || []),
        catchError((err) => {
          console.error('searchEntities error:', err);
          return of([]);
        })
      );
  }

  getEntityTimeline(entityId: string): Observable<any[]> {
    // TODO: Replace with actual API endpoint and params
    return this.http.get<any>(`api/entity/${entityId}/timeline`)
      .pipe(
        timeout(10000),
        retry(this.retryCount),
        map((res: any) => res.timeline || []),
        catchError((err) => {
          console.error('getEntityTimeline error:', err);
          return of([]);
        })
      );
  }

  getAssociatedEntities(entityId: string): Observable<any[]> {
    // TODO: Replace with actual API endpoint and params
    return this.http.get<any>(`api/entity/${entityId}/associated`)
      .pipe(
        timeout(10000),
        retry(this.retryCount),
        map((res: any) => res.associated?.slice(0, 25) || []),
        catchError((err) => {
          console.error('getAssociatedEntities error:', err);
          return of([]);
        })
      );
  }
}
