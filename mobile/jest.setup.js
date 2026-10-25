const { timeoutManager } = require('@tanstack/react-query');

jest.mock('@react-native-async-storage/async-storage', () =>
  require('@react-native-async-storage/async-storage/jest/async-storage-mock'));

// React Query's garbage-collection timers last up to an hour and would keep Jest from exiting.
const unrefed = timer => {
  if (timer && typeof timer.unref === 'function') timer.unref();
  return timer;
};
timeoutManager.setTimeoutProvider({
  setTimeout: (callback, delay) => unrefed(setTimeout(callback, delay)),
  clearTimeout: timer => clearTimeout(timer),
  setInterval: (callback, delay) => unrefed(setInterval(callback, delay)),
  clearInterval: timer => clearInterval(timer),
});

jest.mock('react-native-safe-area-context', () =>
  require('react-native-safe-area-context/jest/mock').default);
