import { useCallback } from 'react';

import { useToast } from '@/components/Toast';
import { errorMessage } from './errors';

/** Runs a user action; on failure shows a toast instead of letting the rejection escape. */
export function useAttempt() {
  const toast = useToast();
  return useCallback(async <T,>(action: () => Promise<T>, failureMessage: string): Promise<T | undefined> => {
    try {
      return await action();
    } catch (error) {
      toast(errorMessage(error, failureMessage));
      return undefined;
    }
  }, [toast]);
}
