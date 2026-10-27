export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  gradeLevel?: string;
}

export interface AuthResponse {
  id: string;
  fullName: string;
  email: string;
  gradeLevel?: string;
  token: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  gradeLevel?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}
