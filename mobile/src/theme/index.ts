import { useColorScheme } from 'react-native';

const light = {
  paper: '#FAFAF8',
  tile: '#ECECE7',
  tile2: '#E2E2DB',
  ink: '#141414',
  ink2: '#686862',
  line: '#DCDCD5',
  accent: '#2B3CD6',
  onAccent: '#FFFFFF',
  scrim: 'rgba(0,0,0,0.32)',
};

const dark: typeof light = {
  paper: '#131313',
  tile: '#1F1F1D',
  tile2: '#292927',
  ink: '#F1F1ED',
  ink2: '#9C9C95',
  line: '#2C2C2A',
  accent: '#8F9BFF',
  onAccent: '#0B0C18',
  scrim: 'rgba(0,0,0,0.55)',
};

export type Palette = typeof light;

export function useTheme(): Palette {
  return useColorScheme() === 'dark' ? dark : light;
}

export const fonts = {
  display: 'BodoniModa_500Medium',
  displayItalic: 'BodoniModa_500Medium_Italic',
  body: 'HankenGrotesk_400Regular',
  bodyMedium: 'HankenGrotesk_500Medium',
  bodySemi: 'HankenGrotesk_600SemiBold',
  mono: 'IBMPlexMono_400Regular',
  monoMedium: 'IBMPlexMono_500Medium',
};

export const gutter = 18;
