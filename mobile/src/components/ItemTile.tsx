import { Pressable, StyleSheet, Text, View, type ViewStyle } from 'react-native';

import type { Item } from '@/domain/types';
import { fonts, useTheme } from '@/theme';
import { Garment } from './Garment';

export function ItemTile({ item, onPress, badge, badgeAccent, selected, width, sub, style }: {
  item: Item; onPress?: () => void; badge?: string; badgeAccent?: boolean; selected?: boolean;
  width?: number; sub?: string; style?: ViewStyle;
}) {
  const t = useTheme();
  return (
    <Pressable onPress={onPress} accessibilityRole="button" accessibilityLabel={item.name}
      style={({ pressed }) => [{ gap: 6, width, opacity: pressed ? 0.85 : 1 }, style]}>
      <View style={[styles.img, { backgroundColor: t.tile }, selected && { borderWidth: 1.5, borderColor: t.ink }]}>
        {badge && (
          <View style={[styles.badge, { backgroundColor: badgeAccent ? t.accent : t.paper }]}>
            <Text style={[styles.badgeText, { color: badgeAccent ? t.onAccent : t.ink }]}>{badge}</Text>
          </View>
        )}
        <Garment item={item} />
      </View>
      <Text style={[styles.name, { color: t.ink }]} numberOfLines={2}>{item.name}</Text>
      {sub !== undefined && <Text style={[styles.sub, { color: t.ink2 }]} numberOfLines={1}>{sub}</Text>}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  img: { aspectRatio: 4 / 5, padding: 16, borderRadius: 3, alignItems: 'center', justifyContent: 'center' },
  badge: { position: 'absolute', top: 8, left: 8, paddingHorizontal: 6, paddingVertical: 3, borderRadius: 2, zIndex: 1 },
  badgeText: { fontFamily: fonts.mono, fontSize: 9, letterSpacing: 0.5, textTransform: 'uppercase' },
  name: { fontFamily: fonts.bodyMedium, fontSize: 13.5, lineHeight: 17 },
  sub: { fontFamily: fonts.body, fontSize: 12 },
});
