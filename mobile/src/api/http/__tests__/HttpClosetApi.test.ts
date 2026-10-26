import { Platform } from 'react-native';

import type { NewItem } from '@/domain/types';
import { ApiError } from '../ApiError';
import { HttpClosetApi } from '../HttpClosetApi';

const BASE = 'http://api.test:5000';

const jsonResponse = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });

const problemResponse = (status: number, body: unknown) =>
  new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/problem+json' } });

function setup(...responses: (Response | Error)[]) {
  const queue = [...responses];
  const fetchMock = jest.fn(async (_url: unknown, _init?: RequestInit) => {
    const next = queue.shift() ?? jsonResponse({});
    if (next instanceof Error) throw next;
    return next;
  });
  const api = new HttpClosetApi({ baseUrl: BASE, fetchImpl: fetchMock as unknown as typeof fetch });
  const lastCall = () => {
    const [url, init] = fetchMock.mock.calls[fetchMock.mock.calls.length - 1];
    return { url: url as string, init: init as RequestInit };
  };
  return { api, fetchMock, lastCall };
}

const outfit = { id: 'o1', title: 'Test', pieces: { top: 't', bottom: 'b', shoes: 's', acc: [] }, saved: false, createdAt: '2026-10-25T08:00:00-04:00' };
const item = {
  id: 'i1', name: 'Shirt', brand: 'B', category: 'shirts', slot: 'top', type: 'shirt', color: '#fff', colorName: 'White',
  formality: 2, fabric: 'cotton', size: 'M', timesWorn: 1, addedAt: '2026-10-01T10:00:00+00:00',
};

describe('HttpClosetApi requests', () => {
  it('strips trailing slashes from the base url', async () => {
    const fetchMock = jest.fn(async (_url: unknown) => jsonResponse({}));
    const api = new HttpClosetApi({ baseUrl: `${BASE}//`, fetchImpl: fetchMock as unknown as typeof fetch });
    await api.getContext();
    expect(fetchMock.mock.calls[0][0]).toBe(`${BASE}/api/context`);
  });

  it('getContext GETs /api/context', async () => {
    const ctx = { date: '2026-10-25', weekday: 'Sunday', tempF: 55, forecast: 'Clear', rain: false };
    const { api, lastCall } = setup(jsonResponse(ctx));
    await expect(api.getContext()).resolves.toEqual(ctx);
    expect(lastCall().url).toBe(`${BASE}/api/context`);
    expect(lastCall().init.method).toBe('GET');
    expect(lastCall().init.body).toBeUndefined();
  });

  it('getProfile resolves the person photo against the base url', async () => {
    const { api } = setup(jsonResponse({ onboarded: true, personPhotoUri: '/media/me.jpg' }));
    await expect(api.getProfile()).resolves.toEqual({ onboarded: true, personPhotoUri: `${BASE}/media/me.jpg` });
  });

  it('drops null optionals from responses', async () => {
    const { api } = setup(jsonResponse({ onboarded: false, personPhotoUri: null }));
    const profile = await api.getProfile();
    expect(profile.personPhotoUri).toBeUndefined();
    expect('personPhotoUri' in profile && profile.personPhotoUri !== undefined).toBe(false);
  });

  it('completeOnboarding PATCHes onboarded: true', async () => {
    const { api, lastCall } = setup(jsonResponse({ onboarded: true }));
    await expect(api.completeOnboarding()).resolves.toMatchObject({ onboarded: true });
    expect(lastCall().url).toBe(`${BASE}/api/profile`);
    expect(lastCall().init.method).toBe('PATCH');
    expect(JSON.parse(lastCall().init.body as string)).toEqual({ onboarded: true });
    expect((lastCall().init.headers as Record<string, string>)['Content-Type']).toBe('application/json');
  });

  it('listItems resolves image urls and trims lastWorn to a date', async () => {
    const { api, lastCall } = setup(jsonResponse([
      { ...item, imageUri: '/media/a.png', lastWorn: '2026-10-20T00:00:00+00:00' },
      { ...item, id: 'i2', imageUri: 'https://cdn.test/b.png' },
      { ...item, id: 'i3' },
    ]));
    const items = await api.listItems();
    expect(lastCall().url).toBe(`${BASE}/api/items`);
    expect(items[0].imageUri).toBe(`${BASE}/media/a.png`);
    expect(items[0].lastWorn).toBe('2026-10-20');
    expect(items[1].imageUri).toBe('https://cdn.test/b.png');
    expect(items[2].imageUri).toBeUndefined();
  });

  it('addItem POSTs the item with integer formality and maps the 201 body', async () => {
    const newItem: NewItem = {
      name: 'Shirt', brand: 'B', category: 'shirts', slot: 'top', type: 'shirt', color: '#fff', colorName: 'White',
      formality: 1.5, fabric: 'cotton', size: 'M', seasons: ['Fall'],
    };
    const { api, lastCall } = setup(jsonResponse({ ...item, imageUri: '/media/x.png' }, 201));
    const saved = await api.addItem(newItem);
    expect(lastCall().init.method).toBe('POST');
    expect(lastCall().url).toBe(`${BASE}/api/items`);
    expect(JSON.parse(lastCall().init.body as string)).toEqual({ ...newItem, formality: 2 });
    expect(saved.imageUri).toBe(`${BASE}/media/x.png`);
  });

  it('todaysOutfit, getOutfit and listSaved use their routes', async () => {
    const { api, fetchMock } = setup(jsonResponse(outfit), jsonResponse(outfit), jsonResponse([outfit]));
    await api.todaysOutfit();
    await api.getOutfit('a/b c');
    await expect(api.listSaved()).resolves.toEqual([outfit]);
    expect(fetchMock.mock.calls.map(c => c[0])).toEqual([
      `${BASE}/api/outfits/today`,
      `${BASE}/api/outfits/a%2Fb%20c`,
      `${BASE}/api/outfits/saved`,
    ]);
  });

  it('styleOutfit POSTs the request body', async () => {
    const { api, lastCall } = setup(jsonResponse(outfit, 201));
    await expect(api.styleOutfit({ prompt: 'dinner', lockItemId: 'i1', avoidRecent: true })).resolves.toEqual(outfit);
    expect(lastCall().url).toBe(`${BASE}/api/outfits/style`);
    expect(lastCall().init.method).toBe('POST');
    expect(JSON.parse(lastCall().init.body as string)).toEqual({ prompt: 'dinner', lockItemId: 'i1', avoidRecent: true });
  });

  it('refineOutfit POSTs the instruction', async () => {
    const { api, lastCall } = setup(jsonResponse(outfit));
    await api.refineOutfit('o1', 'different shoes');
    expect(lastCall().url).toBe(`${BASE}/api/outfits/o1/refine`);
    expect(JSON.parse(lastCall().init.body as string)).toEqual({ instruction: 'different shoes' });
  });

  it('alternatives GETs with the slot query param and maps items', async () => {
    const { api, lastCall } = setup(jsonResponse([{ ...item, imageUri: '/media/s.png' }]));
    const alts = await api.alternatives('o1', 'shoes');
    expect(lastCall().url).toBe(`${BASE}/api/outfits/o1/alternatives?slot=shoes`);
    expect(alts[0].imageUri).toBe(`${BASE}/media/s.png`);
  });

  it('replacePiece PUTs the item id to the slot route', async () => {
    const { api, lastCall } = setup(jsonResponse(outfit));
    await api.replacePiece('o1', 'outer', 'i9');
    expect(lastCall().url).toBe(`${BASE}/api/outfits/o1/pieces/outer`);
    expect(lastCall().init.method).toBe('PUT');
    expect(JSON.parse(lastCall().init.body as string)).toEqual({ itemId: 'i9' });
  });

  it('rejectOutfit POSTs and returns the replacement', async () => {
    const { api, lastCall } = setup(jsonResponse({ ...outfit, id: 'o2' }));
    await expect(api.rejectOutfit('o1')).resolves.toMatchObject({ id: 'o2' });
    expect(lastCall().url).toBe(`${BASE}/api/outfits/o1/reject`);
    expect(lastCall().init.method).toBe('POST');
    expect(lastCall().init.body).toBeUndefined();
  });

  it('setSaved PUTs the flag', async () => {
    const { api, lastCall } = setup(jsonResponse({ ...outfit, saved: true }));
    await expect(api.setSaved('o1', true)).resolves.toMatchObject({ saved: true });
    expect(lastCall().url).toBe(`${BASE}/api/outfits/o1/saved`);
    expect(lastCall().init.method).toBe('PUT');
    expect(JSON.parse(lastCall().init.body as string)).toEqual({ saved: true });
  });

  it('logWear POSTs with no body and returns a date-only entry', async () => {
    const { api, lastCall } = setup(jsonResponse({ date: '2026-10-25T00:00:00-04:00', outfitId: 'o1' }, 201));
    await expect(api.logWear('o1')).resolves.toEqual({ date: '2026-10-25', outfitId: 'o1' });
    expect(lastCall().url).toBe(`${BASE}/api/outfits/o1/wears`);
    expect(lastCall().init.method).toBe('POST');
  });

  it('listWorn GETs /api/wears', async () => {
    const { api, lastCall } = setup(jsonResponse([{ date: '2026-10-25', outfitId: 'o1', outfit }]));
    await expect(api.listWorn()).resolves.toEqual([{ date: '2026-10-25', outfitId: 'o1', outfit }]);
    expect(lastCall().url).toBe(`${BASE}/api/wears`);
  });

  it('resetDemoData POSTs and accepts 204', async () => {
    const { api, lastCall } = setup(new Response(null, { status: 204 }));
    await expect(api.resetDemoData()).resolves.toBeUndefined();
    expect(lastCall().url).toBe(`${BASE}/api/demo/reset`);
    expect(lastCall().init.method).toBe('POST');
  });
});

describe('HttpClosetApi try-on mapping', () => {
  it.each([
    ['ready', 'ready'],
    ['needsPhoto', 'needs-photo'],
    ['failed', 'failed'],
    ['somethingNew', 'failed'],
  ])('maps wire status %s to %s', async (wire, expected) => {
    const { api, lastCall } = setup(jsonResponse({ status: wire }));
    await expect(api.tryOn('o1')).resolves.toMatchObject({ status: expected });
    expect(lastCall().url).toBe(`${BASE}/api/outfits/o1/try-on`);
    expect(lastCall().init.method).toBe('POST');
  });

  it('resolves the image uri and keeps placeholder and message', async () => {
    const { api } = setup(jsonResponse({ status: 'ready', imageUri: '/media/tryon.png', placeholder: true, message: 'hi' }));
    await expect(api.tryOn('o1')).resolves.toEqual({
      status: 'ready', imageUri: `${BASE}/media/tryon.png`, placeholder: true, message: 'hi',
    });
  });
});

describe('HttpClosetApi photo uploads', () => {
  const originalOs = Platform.OS;
  const appendSpy = jest.spyOn(FormData.prototype, 'append');
  beforeEach(() => appendSpy.mockClear());
  afterEach(() => { Platform.OS = originalOs; });

  // Node's FormData stringifies React Native's { uri, name, type } descriptor, so inspect what was appended.
  const photoPart = (_init: RequestInit) => {
    const [field, part] = appendSpy.mock.calls[0] as unknown as [string, { uri: string; name: string; type: string }];
    expect(field).toBe('photo');
    return part;
  };

  it('analyzePhoto sends a multipart photo field without a JSON content type', async () => {
    const { api, lastCall } = setup(jsonResponse({
      name: 'Tee', category: 'shirts', colorName: 'Black', color: '#000', formality: 1, seasons: [], cutoutUri: '/media/cut.png', slot: 'top', type: 'tee',
    }));
    const suggestion = await api.analyzePhoto('file:///tmp/shirt.PNG');
    expect(lastCall().url).toBe(`${BASE}/api/items/analyze`);
    expect(lastCall().init.method).toBe('POST');
    expect(lastCall().init.body).toBeInstanceOf(FormData);
    expect(photoPart(lastCall().init)).toEqual({ uri: 'file:///tmp/shirt.PNG', name: 'photo.png', type: 'image/png' });
    expect((lastCall().init.headers as Record<string, string>)['Content-Type']).toBeUndefined();
    expect(suggestion).toMatchObject({ cutoutUri: `${BASE}/media/cut.png`, slot: 'top', type: 'tee' });
  });

  it('defaults to jpeg for unknown extensions', async () => {
    const { api, lastCall } = setup(jsonResponse({ cutoutUri: 'x' }));
    await api.analyzePhoto('file:///tmp/IMG_0001?x=1');
    expect(photoPart(lastCall().init)).toMatchObject({ name: 'photo.jpg', type: 'image/jpeg' });
  });

  it('setPersonPhoto PUTs a multipart photo and maps the profile', async () => {
    const { api, lastCall } = setup(jsonResponse({ onboarded: false, personPhotoUri: '/media/me.jpg' }));
    const profile = await api.setPersonPhoto('file:///tmp/me.jpeg');
    expect(lastCall().url).toBe(`${BASE}/api/profile/person-photo`);
    expect(lastCall().init.method).toBe('PUT');
    expect(photoPart(lastCall().init)).toMatchObject({ uri: 'file:///tmp/me.jpeg', type: 'image/jpeg' });
    expect(profile.personPhotoUri).toBe(`${BASE}/media/me.jpg`);
  });

  it('setPersonPhoto(null) DELETEs the photo', async () => {
    const { api, lastCall } = setup(jsonResponse({ onboarded: true }));
    await api.setPersonPhoto(null);
    expect(lastCall().url).toBe(`${BASE}/api/profile/person-photo`);
    expect(lastCall().init.method).toBe('DELETE');
    expect(lastCall().init.body).toBeUndefined();
  });

  it('on web, fetches the picked uri and uploads the blob', async () => {
    Platform.OS = 'web';
    const blob = new Blob(['png-bytes'], { type: 'image/png' });
    const blobFetch = jest.fn(async () => ({ blob: async () => blob }));
    const apiFetch = jest.fn(async (_url: unknown, _init?: RequestInit) => jsonResponse({ cutoutUri: 'x' }));
    const routed = ((url: string, init?: RequestInit) => url.startsWith('blob:') ? blobFetch() : apiFetch(url, init)) as unknown as typeof fetch;
    const api = new HttpClosetApi({ baseUrl: BASE, fetchImpl: routed });

    await api.analyzePhoto('blob:http://localhost:8081/abc');

    expect(blobFetch).toHaveBeenCalledTimes(1);
    const file = (apiFetch.mock.calls[0][1]!.body as FormData).get('photo') as File;
    expect(file.name).toBe('photo.png');
    expect(file.type).toBe('image/png');
  });
});

describe('HttpClosetApi errors', () => {
  it('parses problem+json into an ApiError with status and detail', async () => {
    const { api } = setup(problemResponse(404, { title: 'Not Found', status: 404, detail: 'Outfit o9 was not found.' }));
    const error = await api.getOutfit('o9').catch(e => e);
    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({ status: 404, message: 'Outfit o9 was not found.' });
    expect(error.problem.title).toBe('Not Found');
    expect(error.isClientError).toBe(true);
    expect(error.isNetworkError).toBe(false);
  });

  it('falls back to the title when there is no detail', async () => {
    const { api } = setup(problemResponse(502, { title: 'AI provider failed' }));
    await expect(api.styleOutfit({ prompt: 'x' })).rejects.toMatchObject({ status: 502, message: 'AI provider failed' });
  });

  it('uses the first field error for validation problems', async () => {
    const { api } = setup(problemResponse(400, { title: 'One or more validation errors occurred.', errors: { Name: ['Name is required.'], Size: ['Too long.'] } }));
    const error = await api.addItem({} as NewItem).catch(e => e);
    expect(error.status).toBe(400);
    expect(error.message).toBe('Name is required.');
    expect(error.fieldErrors).toEqual({ Name: ['Name is required.'], Size: ['Too long.'] });
  });

  it('handles non-JSON error bodies', async () => {
    const { api } = setup(new Response('<html>Bad gateway</html>', { status: 502 }));
    await expect(api.getContext()).rejects.toMatchObject({ status: 502, message: 'Request failed (502)' });
  });

  it('handles empty error bodies', async () => {
    const { api } = setup(new Response(null, { status: 500 }));
    await expect(api.getContext()).rejects.toMatchObject({ status: 500, message: 'Request failed (500)' });
  });

  it('turns a rejected fetch into a network ApiError', async () => {
    const { api } = setup(new TypeError('Network request failed'));
    const error = await api.getContext().catch(e => e);
    expect(error).toBeInstanceOf(ApiError);
    expect(error.isNetworkError).toBe(true);
    expect(error.message).toContain(BASE);
  });

  it('aborts and reports a timeout when the server is too slow', async () => {
    const fetchMock = jest.fn((_url: unknown, init?: RequestInit) => new Promise<Response>((_resolve, reject) => {
      init!.signal!.addEventListener('abort', () => reject(new Error('aborted')));
    }));
    const api = new HttpClosetApi({ baseUrl: BASE, fetchImpl: fetchMock as unknown as typeof fetch, timeoutMs: 10 });
    const error = await api.getContext().catch(e => e);
    expect(error.isNetworkError).toBe(true);
    expect(error.message).toMatch(/too long/);
  });

  it('gives AI calls the slow timeout instead of the default one', async () => {
    jest.useFakeTimers();
    try {
      const fetchMock = jest.fn(async (_url: unknown, init?: RequestInit) => {
        await new Promise(resolve => setTimeout(resolve, 50));
        return init!.signal!.aborted ? jsonResponse({}, 500) : jsonResponse(outfit);
      });
      const api = new HttpClosetApi({ baseUrl: BASE, fetchImpl: fetchMock as unknown as typeof fetch, timeoutMs: 10, slowTimeoutMs: 1000 });
      const pending = api.styleOutfit({ prompt: 'x' });
      await jest.advanceTimersByTimeAsync(60);
      await expect(pending).resolves.toEqual(outfit);
    } finally {
      jest.useRealTimers();
    }
  });

  it('rejects a 200 response that is not JSON', async () => {
    const { api } = setup(new Response('oops', { status: 200 }));
    await expect(api.getContext()).rejects.toMatchObject({ status: 200, message: expect.stringContaining('could not read') });
  });
});
