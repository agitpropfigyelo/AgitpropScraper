import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TrendingEntity {
  id: string;
  name: string;
  mentionsCountByDate: Record<string, number>;
  totalMentions: number;
}

export interface TrendingResponse {
  trending: TrendingEntity[];
}

@Injectable({
  providedIn: 'root'
})
export class TrendingService {
  private readonly baseUrl = environment.apiUrl;


  constructor(private http: HttpClient) { }

  getTrending(from: string, to: string): Observable<TrendingResponse> {
    const params = { from, to };
    return this.http.get<TrendingResponse>(`${this.baseUrl}/trending`, { params });
  }
}
