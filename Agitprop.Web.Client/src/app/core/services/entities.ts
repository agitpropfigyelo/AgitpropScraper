import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface EntityDto {
  id: string;
  name: string;
}

export interface EntityDetailsDto {
  id?: string;
  name?: string;
  mentionsCountByDate?: Record<string, number>;
  totalMentions: number;
}

export interface EntityDetailsResponse {
  entityId?: string;
  name?: string;
  type?: string;
  totalMentions: number;
}

export interface EntityTimelinePoint {
  date: string;
  count: number;
}

export interface EntityTimelineResponse {
  entityId?: string;
  name?: string;
  timeline: EntityTimelinePoint[];
}

export interface ArticleDto {
  id?: string;
  title?: string;
  articleUrl?: string;
  articlePublishedTime: string;
}

export interface MentioningArticlesResponse {
  articles?: ArticleDto[];
}

export interface EntityCoMentionDto {
  id?: string;
  name?: string;
  coMentionCount: number;
}

export interface RelatedEntityResponse {
  entityId?: string;
  coMentionedEntities?: EntityCoMentionDto[];
}

export interface PaginatedEntitiesResponse {
  entities?: EntityDto[];
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class EntitiesService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // Paginated list of entities
  getEntities(startDate: string, endDate: string, page: number, pageSize: number, search?: string): Observable<PaginatedEntitiesResponse> {
    const params: any = { StartDate: startDate, EndDate: endDate, Page: page, PageSize: pageSize };
    if (search) params.Search = search;
    return this.http.get<PaginatedEntitiesResponse>(`${this.baseUrl}/Entities`, { params });
  }

  // Details for a single entity
  getEntityDetails(entityId: string, startDate: string, endDate: string): Observable<EntityDetailsResponse> {
    const params = { StartDate: startDate, EndDate: endDate };
    return this.http.get<EntityDetailsResponse>(`${this.baseUrl}/Entities/${entityId}/details`, { params });
  }

  // Timeline for a single entity
  getEntityTimeline(entityId: string, startDate: string, endDate: string): Observable<EntityTimelineResponse> {
    const params = { StartDate: startDate, EndDate: endDate };
    return this.http.get<EntityTimelineResponse>(`${this.baseUrl}/Entities/${entityId}/timeline`, { params });
  }

  // Mentioning articles for a single entity
  getEntityArticles(entityId: string, startDate: string, endDate: string): Observable<MentioningArticlesResponse> {
    const params = { StartDate: startDate, EndDate: endDate };
    return this.http.get<MentioningArticlesResponse>(`${this.baseUrl}/Entities/${entityId}/articles`, { params });
  }

  // Related entities
  getRelatedEntities(entityId: string, startDate: string, endDate: string): Observable<RelatedEntityResponse> {
    const params = { StartDate: startDate, EndDate: endDate };
    return this.http.get<RelatedEntityResponse>(`${this.baseUrl}/Entities/${entityId}/related`, { params });
  }

  // Get all entities for autocomplete
  getAllEntities(startDate: string, endDate: string): Observable<EntityDto[]> {
    return this.http.get<EntityDto[]>(`${this.baseUrl}/Entities/all`, { 
      params: { StartDate: startDate, EndDate: endDate }
    });
  }
}
