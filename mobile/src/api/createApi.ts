import type { ClosetApi } from './ClosetApi';
import { HttpClosetApi } from './http/HttpClosetApi';
import { MockClosetApi } from './mock/MockClosetApi';

export function createApi(baseUrl: string | undefined): ClosetApi {
  const trimmed = baseUrl?.trim();
  return trimmed ? new HttpClosetApi({ baseUrl: trimmed }) : new MockClosetApi();
}
