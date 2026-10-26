import { errorMessage } from '../errors';
import { ApiError, NETWORK_ERROR_STATUS } from '../http/ApiError';
import { createQueryClient, shouldRetryQuery } from '../queryClient';

describe('errorMessage', () => {
  it('explains network failures', () => {
    expect(errorMessage(new ApiError(NETWORK_ERROR_STATUS, 'x'), 'fallback')).toMatch(/Can't reach/);
  });

  it('explains AI provider failures', () => {
    expect(errorMessage(new ApiError(502, 'x'), 'fallback')).toMatch(/AI service/);
  });

  it('uses the fallback for other api errors and unknown errors', () => {
    expect(errorMessage(new ApiError(404, 'x'), 'fallback')).toBe('fallback');
    expect(errorMessage(new Error('boom'), 'fallback')).toBe('fallback');
  });
});

describe('query retry policy', () => {
  it('does not retry client errors', () => {
    expect(shouldRetryQuery(0, new ApiError(404, 'missing'))).toBe(false);
  });

  it('retries server and network failures a limited number of times', () => {
    expect(shouldRetryQuery(0, new ApiError(500, 'x'))).toBe(true);
    expect(shouldRetryQuery(1, new ApiError(NETWORK_ERROR_STATUS, 'x'))).toBe(true);
    expect(shouldRetryQuery(2, new ApiError(500, 'x'))).toBe(false);
  });

  it('is installed on the query client', () => {
    expect(createQueryClient().getDefaultOptions().queries?.retry).toBe(shouldRetryQuery);
  });
});
