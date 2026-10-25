import { useEffect, useState } from 'react';
import { AccessibilityInfo, Animated, StyleSheet, View } from 'react-native';

import type { Item } from '@/domain/types';
import { useTheme } from '@/theme';
import { Garment } from './Garment';
import { T } from './ui';

/** Shown while the outfit call runs: the closet flickers as pieces are considered. */
export function StylingOverlay({ items, prompt }: { items: Item[]; prompt: string }) {
  const t = useTheme();
  const shown = items.filter((_, k) => k % 2 === 0).slice(0, 12);
  const [anims] = useState(() => shown.map(() => new Animated.Value(0.25)));

  useEffect(() => {
    let loops: Animated.CompositeAnimation[] = [];
    AccessibilityInfo.isReduceMotionEnabled().then(reduce => {
      if (reduce) return;
      loops = anims.map((a, k) => Animated.loop(Animated.sequence([
        Animated.delay((k * 137) % 1400),
        Animated.timing(a, { toValue: 1, duration: 560, useNativeDriver: true }),
        Animated.timing(a, { toValue: 0.25, duration: 840, useNativeDriver: true }),
      ])));
      loops.forEach(l => l.start());
    });
    return () => loops.forEach(l => l.stop());
  }, [anims]);

  return (
    <View style={[StyleSheet.absoluteFill, styles.wrap, { backgroundColor: t.paper }]} accessibilityLiveRegion="polite">
      <View style={styles.grid}>
        {shown.map((i, k) => (
          <Animated.View key={i.id} style={[styles.cell, { backgroundColor: t.tile, opacity: anims[k] }]}>
            <Garment item={i} />
          </Animated.View>
        ))}
      </View>
      <T variant="displaySm" style={{ fontStyle: 'italic' }}>Going through your {items.length} pieces</T>
      <T variant="label" style={{ textAlign: 'center' }} numberOfLines={2}>{prompt}</T>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: { alignItems: 'center', justifyContent: 'center', gap: 22, padding: 24, zIndex: 10 },
  grid: { flexDirection: 'row', flexWrap: 'wrap', width: 4 * 58 + 3 * 6, gap: 6 },
  cell: { width: 58, aspectRatio: 4 / 5, padding: 6, borderRadius: 2 },
});
