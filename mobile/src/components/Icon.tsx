import { Path, Rect, Circle, Svg } from 'react-native-svg';

export type IconName =
  | 'home' | 'hanger' | 'plus' | 'spark' | 'stack' | 'back' | 'heart' | 'x' | 'again'
  | 'arrow' | 'search' | 'rain' | 'cal' | 'photos' | 'check' | 'camera' | 'person';

const paths: Record<IconName, (c: string, fill: string) => React.ReactNode> = {
  home: c => <><Path d="M4 10.5 12 4l8 6.5V20H4z" /><Path d="M10 20v-5h4v5" /></>,
  hanger: () => <><Path d="M12 7.5a2 2 0 1 1 2-2c0 1.1-2 1.4-2 3v1" /><Path d="M12 9.5 3.3 16.6c-.7.6-.3 1.4.5 1.4h16.4c.8 0 1.2-.8.5-1.4z" /></>,
  plus: () => <Path d="M12 5v14M5 12h14" />,
  spark: () => <Path d="M12 3.5 13.7 10.3 20.5 12l-6.8 1.7L12 20.5l-1.7-6.8L3.5 12l6.8-1.7z" />,
  stack: () => <><Rect x="4" y="5" width="16" height="15" rx="1.5" /><Path d="M4 10h16M9 3v4M15 3v4" /></>,
  back: () => <Path d="M15 5l-7 7 7 7" />,
  heart: (_c, fill) => <Path d="M12 20s-7.5-4.6-7.5-10.2A4.2 4.2 0 0 1 12 7.3a4.2 4.2 0 0 1 7.5 2.5C19.5 15.4 12 20 12 20z" fill={fill} />,
  x: () => <Path d="M6 6l12 12M18 6 6 18" />,
  again: () => <><Path d="M19.5 12a7.5 7.5 0 1 1-2.2-5.3" /><Path d="M19.5 4.5v4.5H15" /></>,
  arrow: () => <Path d="M5 12h14M13 6l6 6-6 6" />,
  search: () => <><Circle cx="11" cy="11" r="6.5" /><Path d="m20 20-4.2-4.2" /></>,
  rain: () => <><Path d="M7 15a4 4 0 0 1 .4-8A5.5 5.5 0 0 1 18 8.5a3.3 3.3 0 0 1-.5 6.5z" /><Path d="M9 18l-1 2M13 18l-1 2M17 18l-1 2" /></>,
  cal: () => <><Rect x="4" y="5" width="16" height="15" rx="1.5" /><Path d="M4 10h16" /></>,
  photos: () => <><Rect x="4" y="5" width="16" height="14" rx="1.5" /><Path d="m4 16 5-5 4 4 2-2 5 5" /></>,
  check: () => <Path d="m5 12.5 4.5 4.5L19 7.5" />,
  camera: () => <><Path d="M4 8h3l2-2.5h6L17 8h3v11H4z" /><Circle cx="12" cy="13" r="3.5" /></>,
  person: () => <><Circle cx="12" cy="6" r="2.5" /><Path d="M8 21v-6l-1.5-.5L8 10h8l1.5 4.5L16 15v6M12 15v6" /></>,
};

export function Icon({ name, color, size = 20, filled = false }: { name: IconName; color: string; size?: number; filled?: boolean }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth={1.6} strokeLinecap="round" strokeLinejoin="round">
      {paths[name](color, filled ? color : 'none')}
    </Svg>
  );
}
