import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import {
  AnswerMode,
  SendMessageRequest,
  SendMessageResponse,
  SessionDetail,
  SessionSummary
} from '../models/chat.model';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  constructor(private http: HttpClient) { }

  getMySessions(): Observable<ApiResponse<SessionSummary[]>> {
    return this.http.get<ApiResponse<SessionSummary[]>>(`${environment.apiUrl}/sessions`);
  }

  getSessionById(sessionId: number): Observable<ApiResponse<SessionDetail>> {
    return this.http.get<ApiResponse<SessionDetail>>(`${environment.apiUrl}/sessions/${sessionId}`);
  }

  createSession(title?: string): Observable<ApiResponse<{ id: number; title: string; createdAt: string }>> {
    return this.http.post<ApiResponse<{ id: number; title: string; createdAt: string }>>(
      `${environment.apiUrl}/sessions`,
      { title: title || 'New Conversation' }
    );
  }

  deleteSession(sessionId: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${environment.apiUrl}/sessions/${sessionId}`);
  }

  sendMessage(
    sessionId: number,
    message: string,
    answerMode: AnswerMode
  ): Observable<ApiResponse<SendMessageResponse>> {
    const payload: SendMessageRequest = { message, answerMode };
    return this.http.post<ApiResponse<SendMessageResponse>>(
      `${environment.apiUrl}/sessions/${sessionId}/messages`,
      payload
    );
  }
}
