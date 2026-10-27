import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AuthResponse, LoginRequest, RegisterRequest, UserProfile } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'eng_tutor_token';
  private readonly USER_KEY = 'eng_tutor_user';

  private currentUserSignal = signal<UserProfile | null>(this.getStoredUser());
  readonly currentUser = computed(() => this.currentUserSignal());
  readonly isAuthenticated = computed(() => !!this.currentUserSignal() && !!this.getToken());

  constructor(private http: HttpClient) {}

  register(data: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/account/register`, data).pipe(
      tap(res => {
        if (res.isSuccess && res.data) {
          this.setSession(res.data);
        }
      })
    );
  }

  login(data: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/account/login`, data).pipe(
      tap(res => {
        if (res.isSuccess && res.data) {
          this.setSession(res.data);
        }
      })
    );
  }

  getProfile(): Observable<ApiResponse<UserProfile>> {
    return this.http.get<ApiResponse<UserProfile>>(`${environment.apiUrl}/account/me`).pipe(
      tap(res => {
        if (res.isSuccess && res.data) {
          this.currentUserSignal.set(res.data);
          localStorage.setItem(this.USER_KEY, JSON.stringify(res.data));
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSignal.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private setSession(auth: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, auth.token);
    const profile: UserProfile = {
      id: auth.id,
      fullName: auth.fullName,
      email: auth.email,
      gradeLevel: auth.gradeLevel
    };
    localStorage.setItem(this.USER_KEY, JSON.stringify(profile));
    this.currentUserSignal.set(profile);
  }

  private getStoredUser(): UserProfile | null {
    const raw = localStorage.getItem(this.USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UserProfile;
    } catch {
      return null;
    }
  }
}
