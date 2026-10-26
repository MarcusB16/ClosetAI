import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { cleanup, fireEvent, render, screen } from '@testing-library/react-native';
import type { ReactElement } from 'react';

import { ApiError, NETWORK_ERROR_STATUS } from '@/api/http/ApiError';
import { ToastProvider } from '@/components/Toast';
import type { Item, Outfit } from '@/domain/types';
import Closet from '@/app/(tabs)/closet';
import TryOn from '@/app/try-on/[id]';

jest.mock('expo-router', () => ({
  router: { push: jest.fn(), back: jest.fn(), replace: jest.fn(), navigate: jest.fn() },
  useLocalSearchParams: () => ({ id: 'o1' }),
}));

const mockApi = {
  listItems: jest.fn(),
  getOutfit: jest.fn(),
  getProfile: jest.fn(),
  tryOn: jest.fn(),
};
jest.mock('@/api', () => ({ get api() { return mockApi; } }));

const item = (overrides: Partial<Item> = {}): Item => ({
  id: 'i1', name: 'White oxford shirt', brand: 'J.Crew', category: 'shirts', slot: 'top', type: 'shirt', color: '#fff', colorName: 'White',
  formality: 2, fabric: 'cotton', size: 'M', timesWorn: 3, addedAt: '2026-01-01T00:00:00Z', ...overrides,
});
const outfit: Outfit = {
  id: 'o1', title: 'Weekend look', pieces: { top: 'i1', bottom: 'i2', shoes: 'i3', acc: [] }, saved: false, createdAt: '2026-10-25T08:00:00Z',
};

const clients: QueryClient[] = [];
function renderScreen(ui: ReactElement) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  clients.push(client);
  return render(<QueryClientProvider client={client}><ToastProvider>{ui}</ToastProvider></QueryClientProvider>);
}

beforeEach(() => Object.values(mockApi).forEach(fn => fn.mockReset()));
afterEach(async () => {
  await cleanup();
  clients.splice(0).forEach(client => client.clear());
});

describe('Closet screen', () => {
  it('shows the pieces once loaded', async () => {
    mockApi.listItems.mockResolvedValue([item()]);
    await renderScreen(<Closet />);
    expect(await screen.findByText('White oxford shirt')).toBeTruthy();
    expect(screen.getByText('1 pieces')).toBeTruthy();
  });

  it('shows an error with a retry that loads the closet', async () => {
    mockApi.listItems.mockRejectedValueOnce(new ApiError(NETWORK_ERROR_STATUS, 'offline')).mockResolvedValueOnce([item()]);
    await renderScreen(<Closet />);

    expect(await screen.findByText("Couldn't load your closet.")).toBeTruthy();
    await fireEvent.press(screen.getByText('Try again'));

    expect(await screen.findByText('White oxford shirt')).toBeTruthy();
    expect(screen.queryByText("Couldn't load your closet.")).toBeNull();
    expect(mockApi.listItems).toHaveBeenCalledTimes(2);
  });
});

describe('Try-on screen', () => {
  beforeEach(() => {
    mockApi.getOutfit.mockResolvedValue(outfit);
    mockApi.getProfile.mockResolvedValue({ onboarded: true, personPhotoUri: 'http://x/me.jpg' });
    mockApi.listItems.mockResolvedValue([item()]);
  });

  it('shows the image once the try-on is ready', async () => {
    mockApi.tryOn.mockResolvedValue({ status: 'ready', imageUri: 'http://x/result.png' });
    await renderScreen(<TryOn />);
    expect(await screen.findByLabelText('You wearing Weekend look')).toBeTruthy();
  });

  it('explains a failed request and retries on demand', async () => {
    mockApi.tryOn.mockRejectedValueOnce(new ApiError(502, 'AI down')).mockResolvedValueOnce({ status: 'ready', imageUri: 'http://x/result.png' });
    await renderScreen(<TryOn />);

    expect(await screen.findByText('Couldn’t make your preview')).toBeTruthy();
    await fireEvent.press(screen.getByText('Try again'));

    expect(await screen.findByLabelText('You wearing Weekend look')).toBeTruthy();
    expect(mockApi.tryOn).toHaveBeenCalledTimes(2);
  });

  it('shows the server message when the image service reports a failure', async () => {
    mockApi.tryOn.mockResolvedValue({ status: 'failed', message: 'The image model is busy.' });
    await renderScreen(<TryOn />);
    expect(await screen.findByText('The image model is busy.')).toBeTruthy();
  });

  it('asks for a photo when the backend reports needs-photo', async () => {
    mockApi.tryOn.mockResolvedValue({ status: 'needs-photo' });
    await renderScreen(<TryOn />);
    expect(await screen.findByText('Add your photo first')).toBeTruthy();
  });

  it('offers a retry when the outfit itself cannot be loaded', async () => {
    mockApi.getOutfit.mockRejectedValueOnce(new ApiError(404, 'missing')).mockResolvedValueOnce(outfit);
    mockApi.tryOn.mockResolvedValue({ status: 'ready', imageUri: 'http://x/result.png' });
    await renderScreen(<TryOn />);

    expect(await screen.findByText("Couldn't load this outfit.")).toBeTruthy();
    await fireEvent.press(screen.getByText('Try again'));
    expect(await screen.findByLabelText('You wearing Weekend look')).toBeTruthy();
  });
});
