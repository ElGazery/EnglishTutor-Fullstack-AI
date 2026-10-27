export interface ApiResponse<T> {
  statusCode: number;
  isSuccess: boolean;
  message: string;
  data: T | null;
  errors?: string[];
}
