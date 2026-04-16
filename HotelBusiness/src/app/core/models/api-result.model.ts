export interface ApiResult<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  code: string | null;
  validationErrors: string[] | null;
}
