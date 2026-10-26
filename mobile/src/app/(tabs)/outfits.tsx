import { router } from 'expo-router';
import { useState } from 'react';
import { Pressable, View } from 'react-native';

import { useItemMap, useSaved, useWorn } from '@/api/hooks';
import { Garment } from '@/components/Garment';
import { Icon } from '@/components/Icon';
import { MiniBoard, PieceRow, resolvePieces } from '@/components/Board';
import { ErrorState, Loading, Screen, T } from '@/components/ui';
import { isoDay, parseDay, weekdayShort } from '@/domain/dates';
import { fonts, useTheme } from '@/theme';

export default function Outfits() {
  const t = useTheme();
  const [tab, setTab] = useState<'worn' | 'saved'>('worn');
  const { data: worn = [], isLoading: wornLoading, isError: wornFailed, refetch: refetchWorn } = useWorn();
  const { data: saved = [], isLoading: savedLoading, isError: savedFailed, refetch: refetchSaved } = useSaved();
  const itemMap = useItemMap();

  const counts = new Map<string, number>();
  worn.forEach(w => [w.outfit.pieces.outer, w.outfit.pieces.top, w.outfit.pieces.bottom, w.outfit.pieces.shoes]
    .forEach(id => id && counts.set(id, (counts.get(id) ?? 0) + 1)));
  const top = [...counts.entries()].sort((a, b) => b[1] - a[1])[0];
  const topItem = top ? itemMap.get(top[0]) : undefined;
  const today = isoDay();

  if (wornLoading && savedLoading) return <Loading />;

  return (
    <Screen>
      <T variant="display">Outfits</T>
      <View style={{ flexDirection: 'row', backgroundColor: t.tile, borderRadius: 999, padding: 3 }}>
        {(['worn', 'saved'] as const).map(k => (
          <Pressable key={k} onPress={() => setTab(k)} accessibilityRole="tab" accessibilityState={{ selected: tab === k }}
            style={{ flex: 1, paddingVertical: 9, borderRadius: 999, alignItems: 'center', backgroundColor: tab === k ? t.paper : 'transparent' }}>
            <T style={{ fontFamily: fonts.bodyMedium, color: tab === k ? t.ink : t.ink2 }}>
              {k === 'worn' ? 'Worn' : `Saved · ${saved.length}`}
            </T>
          </Pressable>
        ))}
      </View>

      {tab === 'worn' ? (
        <View>
          {topItem && top && (
            <View style={{ backgroundColor: t.tile, padding: 14, borderRadius: 3, flexDirection: 'row', gap: 14, alignItems: 'center', marginBottom: 8 }}>
              <View style={{ width: 52, height: 60 }}><Garment item={topItem} /></View>
              <T variant="small" style={{ flex: 1 }}>
                Your <T variant="small" style={{ fontFamily: fonts.bodySemi }}>{topItem.name.toLowerCase()}</T> showed up {top[1]} times in {worn.length} outfits. Ask for “something different” to give it a rest.
              </T>
            </View>
          )}
          {wornFailed && worn.length === 0 && <ErrorState message="Couldn't load your worn outfits." onRetry={() => refetchWorn()} />}
          {!wornFailed && worn.length === 0 && <T tone="muted">Outfits you wear show up here. Tap “Wear this” on any outfit to log it.</T>}
          {worn.map(w => (
            <Pressable key={`${w.date}-${w.outfitId}`} onPress={() => router.push(`/outfit/${w.outfit.id}`)} accessibilityRole="button"
              style={{ flexDirection: 'row', gap: 12, alignItems: 'center', paddingVertical: 10, borderBottomWidth: 1, borderColor: t.line }}>
              <View style={{ width: 44 }}>
                <T variant="label">{weekdayShort(w.date)}</T>
                <T variant="display" style={{ fontSize: 26, lineHeight: 28, color: w.date === today ? t.accent : t.ink }}>{parseDay(w.date).getDate()}</T>
              </View>
              <View style={{ flex: 1, gap: 6 }}>
                <PieceRow pieces={resolvePieces(w.outfit, itemMap)} />
                <T variant="small" tone="muted">{w.outfit.title}</T>
              </View>
            </Pressable>
          ))}
        </View>
      ) : (
        <View style={{ flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between', rowGap: 16 }}>
          {savedFailed && saved.length === 0 && <ErrorState message="Couldn't load your saved outfits." onRetry={() => refetchSaved()} />}
          {!savedFailed && saved.length === 0 && <T tone="muted">Tap the heart on an outfit to save it here.</T>}
          {saved.map(o => (
            <Pressable key={o.id} onPress={() => router.push(`/outfit/${o.id}`)} accessibilityRole="button" style={{ width: '48.5%', gap: 6 }}>
              <MiniBoard pieces={resolvePieces(o, itemMap)} />
              <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' }}>
                <T variant="small" style={{ fontFamily: fonts.bodyMedium, flex: 1 }}>{o.title}</T>
                <Icon name="heart" color={t.accent} size={14} filled />
              </View>
            </Pressable>
          ))}
        </View>
      )}
    </Screen>
  );
}
