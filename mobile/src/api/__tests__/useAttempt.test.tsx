import { act, fireEvent, render, screen } from '@testing-library/react-native';
import { Pressable, Text } from 'react-native';

import { ToastProvider } from '@/components/Toast';
import { ApiError, NETWORK_ERROR_STATUS } from '../http/ApiError';
import { useAttempt } from '../useAttempt';


function Harness({ action, onResult }: { action: () => Promise<string>; onResult?: (result: unknown) => void }) {
  const attempt = useAttempt();
  return (
    <Pressable accessibilityRole="button" onPress={async () => {
      const result = await attempt(action, 'Fallback failure message');
      onResult?.(result);
    }}>
      <Text>Run</Text>
    </Pressable>
  );
}

const renderHarness = (action: () => Promise<string>, onResult?: (result: unknown) => void) =>
  render(<ToastProvider><Harness action={action} onResult={onResult} /></ToastProvider>);

describe('useAttempt', () => {
  beforeEach(() => jest.useFakeTimers());
  afterEach(async () => {
    await act(async () => { await jest.runOnlyPendingTimersAsync(); });
    jest.useRealTimers();
  });

  it('stays quiet when the action succeeds', async () => {
    const action = jest.fn().mockResolvedValue('ok');
    await renderHarness(action);
    await fireEvent.press(screen.getByRole('button'));
    expect(action).toHaveBeenCalledTimes(1);
    expect(screen.queryByText('Fallback failure message')).toBeNull();
  });

  it('toasts the fallback message when the action fails', async () => {
    await renderHarness(jest.fn().mockRejectedValue(new Error('boom')));
    await fireEvent.press(screen.getByRole('button'));
    expect(await screen.findByText('Fallback failure message')).toBeTruthy();
  });

  it('toasts a connection message for network failures', async () => {
    await renderHarness(jest.fn().mockRejectedValue(new ApiError(NETWORK_ERROR_STATUS, 'offline')));
    await fireEvent.press(screen.getByRole('button'));
    expect(await screen.findByText(/Can't reach the Closet server/)).toBeTruthy();
  });

  it('toasts an AI message for 502 failures', async () => {
    await renderHarness(jest.fn().mockRejectedValue(new ApiError(502, 'bad gateway')));
    await fireEvent.press(screen.getByRole('button'));
    expect(await screen.findByText(/AI service/)).toBeTruthy();
  });

  it('resolves to undefined on failure and to the value on success', async () => {
    const results: unknown[] = [];
    await renderHarness(jest.fn().mockRejectedValueOnce(new Error('x')).mockResolvedValueOnce('fine'), r => results.push(r));
    await fireEvent.press(screen.getByRole('button'));
    await fireEvent.press(screen.getByRole('button'));
    await act(async () => {});
    expect(results).toEqual([undefined, 'fine']);
  });
});
