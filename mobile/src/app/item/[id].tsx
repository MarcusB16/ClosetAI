import { router, useLocalSearchParams } from 'expo-router';
import { Pressable, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { useItemMap, useItems, useSaved, useWorn } from '@/api/hooks';
import { MiniBoard, resolvePieces } from '@/components/Board';
import { Garment } from '@/components/Garment';
import { Btn, HScroll, IconBtn, Loading, Screen, SectionHead, T } from '@/components/ui';
import { fmtDay, fmtMonthDay } from '@/domain/dates';
import { startStyling } from '@/domain/navigation';
import { CATEGORY_LABEL, type Item, type Outfit } from '@/domain/types';
import { fonts, gutter, useTheme } from '@/theme';

function careFor(fabric: string): string {
  if (/suede/i.test(fabric)) return 'Brush only. Keep dry.';
  if (/leather|calf|yearling/i.test(fabric)) return 'Wipe clean. Condition monthly.';
  if (/silk|cashmere|flannel|110s/i.test(fabric)) return 'Dry clean only';
  if (/wool|merino|alpaca/i.test(fabric)) return 'Hand wash cold. Dry flat.';
  if (/waxed/i.test(fabric)) return 'Sponge clean. Rewax yearly.';
  if (/denim/i.test(fabric)) return 'Wash cold, inside out';
  return 'Machine wash 86°F';
}

const dressLevel = (f: number) => (f >= 2.8 ? 'Formal' : f >= 2 ? 'Smart' : f >= 1.5 ? 'Smart casual' : 'Casual');

export default function ItemScreen() {
  const t = useTheme();
  const insets = useSafeAreaInsets();
  const { id } = useLocalSearchParams<{ id: string }>();
  const { data: items } = useItems();
  const itemMap = useItemMap();
  const { data: worn = [] } = useWorn();
  const { data: saved = [] } = useSaved();
  const item = items?.find(i => i.id === id);
  if (!item) return <Loading />;

  const uses = (o: Outfit) => [o.pieces.outer, o.pieces.top, o.pieces.bottom, o.pieces.shoes, ...o.pieces.acc].includes(item.id);
  const seen: { outfit: Outfit; date?: string }[] = [
    ...worn.filter(w => uses(w.outfit)).map(w => ({ outfit: w.outfit, date: w.date })),
    ...saved.filter(uses).map(o => ({ outfit: o })),
  ].slice(0, 8);

  const perWear = item.price && item.timesWorn ? `$${(item.price / item.timesWorn).toFixed(2)}` : '—';

  return (
    <View style={{ flex: 1, backgroundColor: t.paper }}>
      <Screen contentStyle={{ paddingBottom: 110 }}>
        <View style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' }}>
          <IconBtn icon="back" label="Back" onPress={() => router.back()} />
          <T variant="label">{CATEGORY_LABEL[item.category]}</T>
          <View style={{ width: 40 }} />
        </View>
        <View style={{ backgroundColor: t.tile, aspectRatio: 1 / 1.02, padding: 34, marginHorizontal: -gutter }}>
          <Garment item={item} />
        </View>
        <View style={{ gap: 6 }}>
          <T variant="display">{item.name}</T>
          <T tone="muted">{item.brand} · Size {item.size}</T>
        </View>
        <Stats item={item} perWear={perWear} />
        <View style={{ gap: 8 }}>
          <Fact label="Color" value={item.colorName} swatch={item.color} />
          <Fact label="Dress level" value={dressLevel(item.formality)} />
        </View>
        <View style={{ alignSelf: 'flex-start', backgroundColor: t.tile2, paddingVertical: 12, paddingHorizontal: 22, borderWidth: 1, borderStyle: 'dashed', borderColor: t.ink2 }}>
          <T style={{ fontFamily: fonts.mono, fontSize: 10.5, letterSpacing: 1, textTransform: 'uppercase', lineHeight: 18 }}>
            {item.fabric}{'\n'}{careFor(item.fabric)}
          </T>
        </View>
        <View style={{ gap: 14 }}>
          <SectionHead title="Seen in" right={<T variant="label">{seen.length} outfit{seen.length === 1 ? '' : 's'}</T>} />
          {seen.length ? (
            <HScroll>
              {seen.map(({ outfit, date }, k) => (
                <Pressable key={outfit.id + k} onPress={() => router.push(`/outfit/${outfit.id}`)} style={{ width: 120, gap: 6 }} accessibilityRole="button">
                  <MiniBoard pieces={resolvePieces(outfit, itemMap)} />
                  <T variant="small">{outfit.title}</T>
                  <T variant="label">{date ? fmtDay(date) : 'Saved'}</T>
                </Pressable>
              ))}
            </HScroll>
          ) : <T tone="muted">Not in any outfit yet. Build one below.</T>}
        </View>
      </Screen>
      <View style={{ position: 'absolute', left: gutter, right: gutter, bottom: insets.bottom + 12 }}>
        <Btn variant="accent" icon="spark" label="Build an outfit around this"
          onPress={() => startStyling(`Build an outfit around my ${item.name.toLowerCase()}.`, item.id)} />
      </View>
    </View>
  );
}

function Stats({ item, perWear }: { item: Item; perWear: string }) {
  const t = useTheme();
  const cells = [
    ['Worn', `${item.timesWorn}×`],
    ['Last worn', item.lastWorn ? fmtMonthDay(item.lastWorn) : 'Never'],
    ['Per wear', perWear],
  ];
  return (
    <View style={{ flexDirection: 'row', borderTopWidth: 1, borderBottomWidth: 1, borderColor: t.line }}>
      {cells.map(([l, v], k) => (
        <View key={l} style={{ flex: 1, paddingVertical: 12, paddingLeft: k ? 12 : 0, borderLeftWidth: k ? 1 : 0, borderColor: t.line, gap: 2 }}>
          <T variant="label">{l}</T>
          <T style={{ fontFamily: fonts.bodySemi, fontSize: 18, fontVariant: ['tabular-nums'] }}>{v}</T>
        </View>
      ))}
    </View>
  );
}

function Fact({ label, value, swatch }: { label: string; value: string; swatch?: string }) {
  const t = useTheme();
  return (
    <View style={{ flexDirection: 'row', justifyContent: 'space-between', paddingBottom: 8, borderBottomWidth: 1, borderColor: t.line }}>
      <T tone="muted">{label}</T>
      <View style={{ flexDirection: 'row', alignItems: 'center', gap: 6 }}>
        {swatch && <View style={{ width: 14, height: 14, borderRadius: 7, backgroundColor: swatch, borderWidth: 1, borderColor: t.line }} />}
        <T>{value}</T>
      </View>
    </View>
  );
}
