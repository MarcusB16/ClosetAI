import type { ClosetApi } from './ClosetApi';
import { createApi } from './createApi';

export const api: ClosetApi = createApi(process.env.EXPO_PUBLIC_API_BASE_URL);

export type { ClosetApi };
