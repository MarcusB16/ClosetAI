import type { ClosetApi } from './ClosetApi';
import { MockClosetApi } from './mock/MockClosetApi';

/** Swap this for an HTTP implementation once the backend exists. */
export const api: ClosetApi = new MockClosetApi();

export type { ClosetApi };
