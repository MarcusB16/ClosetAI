import { QueryClient } from '@tanstack/react-query';

import { ApiError } from './http/ApiError';

const MAX_QUERY_RETRIES = 2;

export function shouldRetryQuery(failureCount: number, error: unknown): boolean {
  if (error instanceof ApiError && error.isClientError) return false;
  return failureCount < MAX_QUERY_RETRIES;
}

export function createQueryClient() {
  return new QueryClient({ defaultOptions: { queries: { retry: shouldRetryQuery } } });
}
