import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { timeout } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TrendingService {
  constructor(private http: HttpClient) {}

  getTrending(): Observable<any[]> {
    // TODO: Replace with actual API endpoint and params
    return this.http.get<any>('api/trending')
      .pipe(
        timeout(10000),
        map((res: any) => res.trending?.slice(0, 10) || []),
        catchError(() => of([]))
      );
  }
}
