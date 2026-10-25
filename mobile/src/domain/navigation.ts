import { router } from 'expo-router';

/** Opens the Style tab and starts styling right away. `n` makes each request unique. */
export function startStyling(prompt: string, lockItemId?: string) {
  router.navigate({ pathname: '/style', params: { prompt, lock: lockItemId ?? '', n: String(Date.now()) } });
}
