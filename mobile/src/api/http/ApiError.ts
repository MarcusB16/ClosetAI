export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

export const NETWORK_ERROR_STATUS = 0;

export class ApiError extends Error {
  readonly status: number;
  readonly problem?: ProblemDetails;

  constructor(status: number, message: string, problem?: ProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }

  get isNetworkError() {
    return this.status === NETWORK_ERROR_STATUS;
  }

  get isClientError() {
    return this.status >= 400 && this.status < 500;
  }

  get fieldErrors(): Record<string, string[]> {
    return this.problem?.errors ?? {};
  }
}

export function messageFromProblem(status: number, problem: ProblemDetails | undefined): string {
  const firstFieldError = Object.values(problem?.errors ?? {}).flat()[0];
  return problem?.detail || firstFieldError || problem?.title || `Request failed (${status})`;
}

export async function readApiError(response: Response): Promise<ApiError> {
  const problem = await readProblem(response);
  return new ApiError(response.status, messageFromProblem(response.status, problem), problem);
}

async function readProblem(response: Response): Promise<ProblemDetails | undefined> {
  const text = await response.text().catch(() => '');
  if (!text) return undefined;
  try {
    const parsed: unknown = JSON.parse(text);
    return parsed && typeof parsed === 'object' ? (parsed as ProblemDetails) : undefined;
  } catch {
    return undefined;
  }
}
