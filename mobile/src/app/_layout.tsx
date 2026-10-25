import {
  BodoniModa_500Medium, BodoniModa_500Medium_Italic,
} from '@expo-google-fonts/bodoni-moda';
import {
  HankenGrotesk_400Regular, HankenGrotesk_500Medium, HankenGrotesk_600SemiBold,
} from '@expo-google-fonts/hanken-grotesk';
import { IBMPlexMono_400Regular, IBMPlexMono_500Medium } from '@expo-google-fonts/ibm-plex-mono';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useFonts } from 'expo-font';
import { Stack } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import { StatusBar } from 'expo-status-bar';
import { useEffect, useState } from 'react';
import { SafeAreaProvider } from 'react-native-safe-area-context';

import { ToastProvider } from '@/components/Toast';
import { useTheme } from '@/theme';

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const t = useTheme();
  const [queryClient] = useState(() => new QueryClient({ defaultOptions: { queries: { retry: 1 } } }));
  const [loaded] = useFonts({
    BodoniModa_500Medium, BodoniModa_500Medium_Italic,
    HankenGrotesk_400Regular, HankenGrotesk_500Medium, HankenGrotesk_600SemiBold,
    IBMPlexMono_400Regular, IBMPlexMono_500Medium,
  });

  useEffect(() => {
    if (loaded) SplashScreen.hideAsync();
  }, [loaded]);

  if (!loaded) return null;

  return (
    <SafeAreaProvider>
      <QueryClientProvider client={queryClient}>
        <ToastProvider>
          <StatusBar style="auto" />
          <Stack screenOptions={{ headerShown: false, contentStyle: { backgroundColor: t.paper } }}>
            <Stack.Screen name="(tabs)" />
            <Stack.Screen name="outfit/[id]" />
            <Stack.Screen name="item/[id]" />
            <Stack.Screen name="add" options={{ presentation: 'modal' }} />
            <Stack.Screen name="try-on/[id]" options={{ presentation: 'modal' }} />
            <Stack.Screen name="setup" options={{ gestureEnabled: false }} />
          </Stack>
        </ToastProvider>
      </QueryClientProvider>
    </SafeAreaProvider>
  );
}
