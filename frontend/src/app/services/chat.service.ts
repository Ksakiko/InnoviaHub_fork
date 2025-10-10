import { Injectable } from '@angular/core';
import { AppConfigService } from '../core/app-config.service';
import { catchError, Observable, throwError } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ChatMessageRequest } from '../../types/chat-types';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private readonly url: string;

  constructor(private http: HttpClient, private cfg: AppConfigService) {
    this.url = `${this.cfg.apiUrl}/api/chat`;
  }

  postChatMessage(request: ChatMessageRequest): Observable<any> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
      }),
      body: JSON.stringify(request),
      observe: 'response' as const,
    };

    return this.http.post<any>(this.url, request, httpOptions).pipe(
      catchError((error) => {
        return throwError(() => error);
      })
    );
  }
}
