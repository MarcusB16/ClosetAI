import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { act, cleanup, renderHook, waitFor } from '@testing-library/react-native';
import type { ReactNode } from 'react';

import type { Outfit } from '@/domain/types';
import {
  keys, useItems, useLogWear, useReplacePiece, useSetPersonPhoto, useSetSaved, useStyleOutfit, useTryOn,
} from '../hooks';
import { ApiError } from '../http/ApiError';

const mockApi = {
  listItems: jest.fn(),
  styleOutfit: jest.fn(),
  replacePiece: jest.fn(),
  logWear: jest.fn(),
  setSaved: jest.fn(),
  setPersonPhoto: jest.fn(),
  tryOn: jest.fn(),
};
jest.mock('../index', () => ({ get api() { return mockApi; } }));

const outfit = (overrides: Partial<Outfit> = {}): Outfit => ({
  id: 'o1', title: 'Look', pieces: { top: 't', bottom: 'b', shoes: 's', acc: [] }, saved: false, createdAt: '2026-10-25T08:00:00Z', ...overrides,
});

const clients: QueryClient[] = [];

function setup() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  clients.push(client);
  const wrapper = ({ children }: { children: ReactNode }) => <QueryClientProvider client={client}>{children}</QueryClientProvider>;
  return { client, wrapper };
}

beforeEach(() => jest.resetAllMocks());
afterEach(async () => {
  await cleanup();
  clients.splice(0).forEach(client => client.clear());
});

describe('query hooks', () => {
  it('exposes the ApiError when the request fails', async () => {
    mockApi.listItems.mockRejectedValue(new ApiError(500, 'boom'));
    const { wrapper } = setup();
    const { result } = await renderHook(() => useItems(), { wrapper });
    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toBeInstanceOf(ApiError);
  });

  it('recovers when refetched after a failure', async () => {
    mockApi.listItems.mockRejectedValueOnce(new ApiError(0, 'offline')).mockResolvedValueOnce([]);
    const { wrapper } = setup();
    const { result } = await renderHook(() => useItems(), { wrapper });
    await waitFor(() => expect(result.current.isError).toBe(true));
    await act(async () => { await result.current.refetch(); });
    await waitFor(() => expect(result.current.data).toEqual([]));
  });
});

describe('outfit mutations', () => {
  it('styleOutfit stores the outfit and refreshes saved and today', async () => {
    const created = outfit({ id: 'new' });
    mockApi.styleOutfit.mockResolvedValue(created);
    const { client, wrapper } = setup();
    const invalidate = jest.spyOn(client, 'invalidateQueries');
    const { result } = await renderHook(() => useStyleOutfit(), { wrapper });

    await act(async () => { await result.current.mutateAsync({ prompt: 'dinner' }); });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockApi.styleOutfit).toHaveBeenCalledWith({ prompt: 'dinner' });
    expect(client.getQueryData(keys.outfit('new'))).toEqual(created);
    expect(invalidate).toHaveBeenCalledWith({ queryKey: keys.saved });
    expect(invalidate).toHaveBeenCalledWith({ queryKey: keys.today });
  });

  it('replacePiece updates the outfit and invalidates its alternatives', async () => {
    const swapped = outfit({ pieces: { top: 't2', bottom: 'b', shoes: 's', acc: [] } });
    mockApi.replacePiece.mockResolvedValue(swapped);
    const { client, wrapper } = setup();
    const invalidate = jest.spyOn(client, 'invalidateQueries');
    const { result } = await renderHook(() => useReplacePiece(), { wrapper });

    await act(async () => { await result.current.mutateAsync({ id: 'o1', slot: 'top', itemId: 't2' }); });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockApi.replacePiece).toHaveBeenCalledWith('o1', 'top', 't2');
    expect(client.getQueryData(keys.outfit('o1'))).toEqual(swapped);
    expect(invalidate).toHaveBeenCalledWith({ queryKey: ['alternatives', 'o1'] });
  });

  it('setSaved sends the new flag and refreshes saved outfits', async () => {
    mockApi.setSaved.mockResolvedValue(outfit({ saved: true }));
    const { client, wrapper } = setup();
    const invalidate = jest.spyOn(client, 'invalidateQueries');
    const { result } = await renderHook(() => useSetSaved(), { wrapper });

    await act(async () => { await result.current.mutateAsync({ id: 'o1', saved: true }); });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockApi.setSaved).toHaveBeenCalledWith('o1', true);
    expect(invalidate).toHaveBeenCalledWith({ queryKey: keys.saved });
  });

  it('logWear refreshes the worn list and item wear counts', async () => {
    mockApi.logWear.mockResolvedValue({ date: '2026-10-25', outfitId: 'o1' });
    const { client, wrapper } = setup();
    const invalidate = jest.spyOn(client, 'invalidateQueries');
    const { result } = await renderHook(() => useLogWear(), { wrapper });

    await act(async () => { await result.current.mutateAsync('o1'); });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(invalidate).toHaveBeenCalledWith({ queryKey: keys.worn });
    expect(invalidate).toHaveBeenCalledWith({ queryKey: keys.items });
  });

  it('a failed mutation rejects without touching the cache', async () => {
    mockApi.styleOutfit.mockRejectedValue(new ApiError(502, 'AI down'));
    const { client, wrapper } = setup();
    const { result } = await renderHook(() => useStyleOutfit(), { wrapper });

    await act(async () => { await result.current.mutateAsync({ prompt: 'x' }).catch(() => undefined); });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(client.getQueryCache().getAll()).toHaveLength(0);
  });
});

describe('useSetPersonPhoto', () => {
  it('stores the profile and drops cached try-on previews', async () => {
    mockApi.setPersonPhoto.mockResolvedValue({ onboarded: true, personPhotoUri: 'http://x/me.jpg' });
    const { client, wrapper } = setup();
    client.setQueryData(['tryOn', 'o1', '{}'], { status: 'ready' });
    const { result } = await renderHook(() => useSetPersonPhoto(), { wrapper });

    await act(async () => { await result.current.mutateAsync('file:///me.jpg'); });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.getQueryData(keys.profile)).toEqual({ onboarded: true, personPhotoUri: 'http://x/me.jpg' });
    expect(client.getQueryData(['tryOn', 'o1', '{}'])).toBeUndefined();
  });
});

describe('useTryOn', () => {
  const remountWith = async (wrapper: ReturnType<typeof setup>['wrapper'], target: Outfit) => {
    const hook = await renderHook(() => useTryOn(target), { wrapper });
    await waitFor(() => expect(hook.result.current.isFetching).toBe(false));
  };

  it('does not call the api until an outfit is available', async () => {
    const { wrapper } = setup();
    await renderHook(() => useTryOn(undefined), { wrapper });
    expect(mockApi.tryOn).not.toHaveBeenCalled();
  });

  it('reuses a ready result when the same look is opened again', async () => {
    mockApi.tryOn.mockResolvedValue({ status: 'ready', imageUri: 'http://x/a.png' });
    const { wrapper } = setup();
    const target = outfit();
    await remountWith(wrapper, target);
    await remountWith(wrapper, target);
    expect(mockApi.tryOn).toHaveBeenCalledTimes(1);
  });

  it('asks again when the previous attempt failed', async () => {
    mockApi.tryOn.mockResolvedValue({ status: 'failed', message: 'model busy' });
    const { wrapper } = setup();
    const target = outfit();
    await remountWith(wrapper, target);
    await remountWith(wrapper, target);
    expect(mockApi.tryOn).toHaveBeenCalledTimes(2);
  });

  it('asks again when the previous attempt needed a photo', async () => {
    mockApi.tryOn.mockResolvedValue({ status: 'needs-photo' });
    const { wrapper } = setup();
    const target = outfit();
    await remountWith(wrapper, target);
    await remountWith(wrapper, target);
    expect(mockApi.tryOn).toHaveBeenCalledTimes(2);
  });

  it('asks again when the pieces changed', async () => {
    mockApi.tryOn.mockResolvedValue({ status: 'ready' });
    const { wrapper } = setup();
    await remountWith(wrapper, outfit());
    await remountWith(wrapper, outfit({ pieces: { top: 'other', bottom: 'b', shoes: 's', acc: [] } }));
    expect(mockApi.tryOn).toHaveBeenCalledTimes(2);
  });

  it('surfaces a rejected request as an error without retrying', async () => {
    mockApi.tryOn.mockRejectedValue(new ApiError(502, 'AI down'));
    const { wrapper } = setup();
    const { result } = await renderHook(() => useTryOn(outfit()), { wrapper });
    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(mockApi.tryOn).toHaveBeenCalledTimes(1);
  });
});
