import type {
  Item, ItemSuggestion, NewItem, Outfit, Profile, Slot, StyleContext, StyleRequest, TryOnResult, WearEntry,
} from '@/domain/types';
import type { ClosetApi } from '../ClosetApi';
import { ApiError, NETWORK_ERROR_STATUS, readApiError } from './ApiError';
import {
  toItem, toProfile, toSuggestion, toTryOnResult, toWearEntry, toWornOutfit,
  type WireTryOnResult, type WireWornOutfit,
} from './mappers';
import { stripTrailingSlashes } from './mediaUrl';
import { buildPhotoForm } from './photoForm';

const API_PREFIX = '/api';
const DEFAULT_TIMEOUT_MS = 20_000;
export const SLOW_TIMEOUT_MS = 120_000;

export interface HttpClosetApiOptions {
  baseUrl: string;
  fetchImpl?: typeof fetch;
  timeoutMs?: number;
  slowTimeoutMs?: number;
}

interface RequestOptions {
  body?: unknown;
  form?: FormData;
  query?: Record<string, string>;
  slow?: boolean;
}

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

const segment = encodeURIComponent;

/** ASP.NET omits or nulls absent optionals; the app's types expect them missing. */
const dropNulls = (_key: string, value: unknown) => (value === null ? undefined : value);

export class HttpClosetApi implements ClosetApi {
  private readonly baseUrl: string;
  private readonly fetchImpl: typeof fetch;
  private readonly timeoutMs: number;
  private readonly slowTimeoutMs: number;

  constructor({ baseUrl, fetchImpl, timeoutMs = DEFAULT_TIMEOUT_MS, slowTimeoutMs = SLOW_TIMEOUT_MS }: HttpClosetApiOptions) {
    this.baseUrl = stripTrailingSlashes(baseUrl);
    this.fetchImpl = fetchImpl ?? ((input, init) => fetch(input, init));
    this.timeoutMs = timeoutMs;
    this.slowTimeoutMs = slowTimeoutMs;
  }

  getContext() {
    return this.request<StyleContext>('GET', '/context');
  }

  async getProfile() {
    return toProfile(this.baseUrl, await this.request<Profile>('GET', '/profile'));
  }

  async setPersonPhoto(photoUri: string | null) {
    const profile = photoUri === null
      ? await this.request<Profile>('DELETE', '/profile/person-photo')
      : await this.request<Profile>('PUT', '/profile/person-photo', { form: await buildPhotoForm(photoUri, this.fetchImpl), slow: true });
    return toProfile(this.baseUrl, profile);
  }

  async completeOnboarding() {
    return toProfile(this.baseUrl, await this.request<Profile>('PATCH', '/profile', { body: { onboarded: true } }));
  }

  async listItems() {
    const items = await this.request<Item[]>('GET', '/items');
    return items.map(item => toItem(this.baseUrl, item));
  }

  async addItem(item: NewItem) {
    const body = { ...item, formality: Math.round(item.formality) };
    return toItem(this.baseUrl, await this.request<Item>('POST', '/items', { body }));
  }

  async analyzePhoto(photoUri: string) {
    const form = await buildPhotoForm(photoUri, this.fetchImpl);
    const suggestion = await this.request<ItemSuggestion>('POST', '/items/analyze', { form, slow: true });
    return toSuggestion(this.baseUrl, suggestion);
  }

  todaysOutfit() {
    return this.request<Outfit>('GET', '/outfits/today');
  }

  getOutfit(id: string) {
    return this.request<Outfit>('GET', `/outfits/${segment(id)}`);
  }

  styleOutfit(request: StyleRequest) {
    return this.request<Outfit>('POST', '/outfits/style', { body: request, slow: true });
  }

  refineOutfit(outfitId: string, instruction: string) {
    return this.request<Outfit>('POST', `/outfits/${segment(outfitId)}/refine`, { body: { instruction }, slow: true });
  }

  async alternatives(outfitId: string, slot: Slot) {
    const items = await this.request<Item[]>('GET', `/outfits/${segment(outfitId)}/alternatives`, { query: { slot } });
    return items.map(item => toItem(this.baseUrl, item));
  }

  replacePiece(outfitId: string, slot: Slot, itemId: string) {
    return this.request<Outfit>('PUT', `/outfits/${segment(outfitId)}/pieces/${segment(slot)}`, { body: { itemId } });
  }

  rejectOutfit(outfitId: string) {
    return this.request<Outfit>('POST', `/outfits/${segment(outfitId)}/reject`, { slow: true });
  }

  setSaved(outfitId: string, saved: boolean) {
    return this.request<Outfit>('PUT', `/outfits/${segment(outfitId)}/saved`, { body: { saved } });
  }

  listSaved() {
    return this.request<Outfit[]>('GET', '/outfits/saved');
  }

  async logWear(outfitId: string): Promise<WearEntry> {
    return toWearEntry(await this.request<WearEntry>('POST', `/outfits/${segment(outfitId)}/wears`));
  }

  async listWorn() {
    const worn = await this.request<WireWornOutfit[]>('GET', '/wears');
    return worn.map(toWornOutfit);
  }

  async tryOn(outfitId: string): Promise<TryOnResult> {
    const result = await this.request<WireTryOnResult>('POST', `/outfits/${segment(outfitId)}/try-on`, { slow: true });
    return toTryOnResult(this.baseUrl, result);
  }

  async resetDemoData() {
    await this.request<void>('POST', '/demo/reset');
  }

  private async request<T>(method: HttpMethod, path: string, options: RequestOptions = {}): Promise<T> {
    const response = await this.send(method, path, options);
    if (!response.ok) throw await readApiError(response);
    return await this.readBody<T>(response);
  }

  private async readBody<T>(response: Response): Promise<T> {
    const text = response.status === 204 ? '' : await response.text();
    if (!text) return undefined as T;
    try {
      return JSON.parse(text, dropNulls) as T;
    } catch {
      throw new ApiError(response.status, 'The server sent a response the app could not read.');
    }
  }

  private async send(method: HttpMethod, path: string, { body, form, query, slow }: RequestOptions): Promise<Response> {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), slow ? this.slowTimeoutMs : this.timeoutMs);
    const headers: Record<string, string> = { Accept: 'application/json, application/problem+json' };
    if (body !== undefined) headers['Content-Type'] = 'application/json';
    try {
      return await this.fetchImpl(this.urlFor(path, query), {
        method,
        headers,
        body: form ?? (body === undefined ? undefined : JSON.stringify(body)),
        signal: controller.signal,
      });
    } catch {
      throw new ApiError(NETWORK_ERROR_STATUS, controller.signal.aborted
        ? 'The server took too long to respond.'
        : `Could not reach the server at ${this.baseUrl}.`);
    } finally {
      clearTimeout(timer);
    }
  }

  private urlFor(path: string, query?: Record<string, string>) {
    const search = query ? `?${new URLSearchParams(query).toString()}` : '';
    return `${this.baseUrl}${API_PREFIX}${path}${search}`;
  }
}
