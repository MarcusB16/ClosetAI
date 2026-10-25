import { ApiError } from './http/ApiError';

const BAD_GATEWAY = 502;

export function errorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof ApiError)) return fallback;
  if (error.isNetworkError) return "Can't reach the Closet server. Check your connection and try again.";
  if (error.status === BAD_GATEWAY) return 'The AI service is having trouble right now. Try again in a moment.';
  return fallback;
}
