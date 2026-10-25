import { forwardRef } from 'react';
import {
  ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, TextInput, View,
  type PressableProps, type ScrollViewProps, type TextInputProps, type TextProps, type ViewStyle,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { fonts, gutter, useTheme } from '@/theme';
import { Icon, type IconName } from './Icon';

type Tone = 'ink' | 'muted' | 'accent' | 'paper';

function useTone(tone: Tone = 'ink') {
  const t = useTheme();
  return { ink: t.ink, muted: t.ink2, accent: t.accent, paper: t.paper }[tone];
}

export function T({ style, tone, variant = 'body', ...rest }: TextProps & { tone?: Tone; variant?: keyof typeof type }) {
  const color = useTone(tone ?? (variant === 'label' ? 'muted' : 'ink'));
  return <Text {...rest} style={[type[variant], { color }, style]} />;
}

const type = StyleSheet.create({
  hero: { fontFamily: fonts.display, fontSize: 44, lineHeight: 44, letterSpacing: -0.9 },
  display: { fontFamily: fonts.display, fontSize: 34, lineHeight: 36, letterSpacing: -0.5 },
  displaySm: { fontFamily: fonts.display, fontSize: 24, lineHeight: 28 },
  quote: { fontFamily: fonts.displayItalic, fontSize: 16, lineHeight: 22 },
  body: { fontFamily: fonts.body, fontSize: 15, lineHeight: 21 },
  small: { fontFamily: fonts.body, fontSize: 13, lineHeight: 18 },
  strong: { fontFamily: fonts.bodySemi, fontSize: 15, lineHeight: 21 },
  label: { fontFamily: fonts.mono, fontSize: 10.5, letterSpacing: 0.9, textTransform: 'uppercase' },
});

export function Screen({ children, contentStyle, ...rest }: ScrollViewProps & { contentStyle?: ViewStyle }) {
  const t = useTheme();
  const insets = useSafeAreaInsets();
  return (
    <ScrollView
      {...rest}
      style={{ flex: 1, backgroundColor: t.paper }}
      contentContainerStyle={[{ paddingTop: insets.top + 6, paddingHorizontal: gutter, paddingBottom: 36, gap: 22 }, contentStyle]}
      keyboardShouldPersistTaps="handled"
      showsVerticalScrollIndicator={false}
    >
      {children}
    </ScrollView>
  );
}

export function Btn({
  label, icon, variant = 'primary', loading, style, ...rest
}: PressableProps & { label: string; icon?: IconName; variant?: 'primary' | 'accent' | 'ghost'; loading?: boolean; style?: ViewStyle }) {
  const t = useTheme();
  const bg = variant === 'primary' ? t.ink : variant === 'accent' ? t.accent : 'transparent';
  const fg = variant === 'primary' ? t.paper : variant === 'accent' ? t.onAccent : t.ink;
  return (
    <Pressable
      accessibilityRole="button"
      {...rest}
      style={({ pressed }) => [
        s.btn, { backgroundColor: bg, opacity: pressed ? 0.8 : 1 },
        variant === 'ghost' && { borderWidth: 1, borderColor: t.line }, style,
      ]}
    >
      {loading ? <ActivityIndicator color={fg} /> : icon ? <Icon name={icon} color={fg} size={18} /> : null}
      <Text style={[s.btnText, { color: fg }]}>{label}</Text>
    </Pressable>
  );
}

export function RoundBtn({ icon, active, label, onPress }: { icon: IconName; active?: boolean; label: string; onPress: () => void }) {
  const t = useTheme();
  return (
    <Pressable
      onPress={onPress}
      accessibilityRole="button"
      accessibilityLabel={label}
      hitSlop={6}
      style={({ pressed }) => [s.round, { borderColor: active ? t.accent : t.line, opacity: pressed ? 0.7 : 1 }]}
    >
      <Icon name={icon} color={active ? t.accent : t.ink} filled={active} />
    </Pressable>
  );
}

export function IconBtn({ icon, label, onPress, active }: { icon: IconName; label: string; onPress: () => void; active?: boolean }) {
  const t = useTheme();
  return (
    <Pressable onPress={onPress} accessibilityRole="button" accessibilityLabel={label} hitSlop={8}
      style={({ pressed }) => [s.iconBtn, pressed && { backgroundColor: t.tile }]}>
      <Icon name={icon} color={active ? t.accent : t.ink} filled={active} />
    </Pressable>
  );
}

export function Chip({ label, selected, onPress, icon }: { label: string; selected?: boolean; onPress?: () => void; icon?: IconName }) {
  const t = useTheme();
  const fg = selected ? t.paper : t.ink;
  return (
    <Pressable
      onPress={onPress}
      disabled={!onPress}
      accessibilityRole={onPress ? 'button' : 'text'}
      accessibilityState={{ selected }}
      style={[s.chip, { borderColor: selected ? t.ink : t.line, backgroundColor: selected ? t.ink : t.paper }]}
    >
      {icon && <Icon name={icon} color={fg} size={15} />}
      <Text style={[s.chipText, { color: fg }]}>{label}</Text>
    </Pressable>
  );
}

export function HScroll({ children, gap = 10 }: { children: React.ReactNode; gap?: number }) {
  return (
    <ScrollView horizontal showsHorizontalScrollIndicator={false} style={{ marginHorizontal: -gutter }}
      contentContainerStyle={{ paddingHorizontal: gutter, gap }} keyboardShouldPersistTaps="handled">
      {children}
    </ScrollView>
  );
}

export const PillInput = forwardRef<TextInput, TextInputProps & { onSubmit: () => void; underline?: boolean }>(
  function PillInput({ onSubmit, underline, style, ...rest }, ref) {
    const t = useTheme();
    return (
      <View style={[underline ? [s.underline, { borderColor: t.ink }] : [s.pill, { borderColor: t.line }]]}>
        <TextInput
          ref={ref}
          placeholderTextColor={t.ink2}
          returnKeyType="go"
          onSubmitEditing={onSubmit}
          {...rest}
          style={[{ flex: 1, color: t.ink, fontFamily: fonts.body, fontSize: underline ? 17 : 15, paddingVertical: 10 }, style]}
        />
        <Pressable onPress={onSubmit} accessibilityRole="button" accessibilityLabel="Go"
          style={[s.go, { backgroundColor: t.ink }, !underline && { width: 34, height: 34 }]}>
          <Icon name="arrow" color={t.paper} size={18} />
        </Pressable>
      </View>
    );
  },
);

export function Row({ children, style, gap = 10 }: { children: React.ReactNode; style?: ViewStyle; gap?: number }) {
  return <View style={[{ flexDirection: 'row', alignItems: 'center', gap, flexWrap: 'wrap' }, style]}>{children}</View>;
}

export function SectionHead({ title, right }: { title: string; right?: React.ReactNode }) {
  return (
    <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'baseline' }}>
      <T variant="strong">{title}</T>
      {right}
    </View>
  );
}

export function Loading() {
  const t = useTheme();
  return <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: t.paper }}><ActivityIndicator color={t.ink2} /></View>;
}

const s = StyleSheet.create({
  btn: { borderRadius: 999, paddingVertical: 13, paddingHorizontal: 20, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: 8 },
  btnText: { fontFamily: fonts.bodySemi, fontSize: 15 },
  round: { width: 48, height: 48, borderRadius: 24, borderWidth: 1, alignItems: 'center', justifyContent: 'center' },
  iconBtn: { width: 40, height: 40, borderRadius: 20, alignItems: 'center', justifyContent: 'center' },
  chip: { borderWidth: 1, borderRadius: 999, paddingVertical: 7, paddingHorizontal: 13, flexDirection: 'row', alignItems: 'center', gap: 6 },
  chipText: { fontFamily: fonts.body, fontSize: 13.5 },
  underline: { flexDirection: 'row', alignItems: 'center', gap: 8, borderBottomWidth: 1.5, paddingVertical: 4 },
  pill: { flexDirection: 'row', alignItems: 'center', gap: 8, borderWidth: 1, borderRadius: 999, paddingLeft: 16, paddingRight: 4, paddingVertical: 2 },
  go: { width: 40, height: 40, borderRadius: 20, alignItems: 'center', justifyContent: 'center' },
});
