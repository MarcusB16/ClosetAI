import { Pressable, StyleSheet, Text, View, type ViewStyle } from 'react-native';

import { SLOT_LABEL, type Item, type Outfit, type Slot } from '@/domain/types';
import { fonts, useTheme } from '@/theme';
import { Garment } from './Garment';

type Resolved = { outer?: Item; top?: Item; bottom?: Item; shoes?: Item; acc: Item[] };

export function resolvePieces(o: Outfit, items: Map<string, Item>): Resolved {
  const get = (id?: string) => (id ? items.get(id) : undefined);
  return {
    outer: get(o.pieces.outer), top: get(o.pieces.top), bottom: get(o.pieces.bottom), shoes: get(o.pieces.shoes),
    acc: o.pieces.acc.map(id => items.get(id)).filter(Boolean) as Item[],
  };
}

/** The flat-lay: outerwear tall on the left, top over bottom, accessories over shoes. */
export function Board({ pieces, captions, onPressSlot, style }: {
  pieces: Resolved; captions?: boolean; onPressSlot?: (slot: Slot) => void; style?: ViewStyle;
}) {
  const t = useTheme();
  const cell = (slot: Slot, items: Item[], flex: number) => {
    const label = items.map(i => i.name).join(', ');
    const inner = (
      <>
        <View style={{ flex: 1, width: '100%', gap: 2 }}>
          {items.map(i => <Garment key={i.id} item={i} />)}
        </View>
        {captions && items.length > 0 && (
          <View style={styles.cap} pointerEvents="none">
            <Text style={[styles.capSlot, { color: t.ink }]}>{SLOT_LABEL[slot]}</Text>
            <Text style={[styles.capName, { color: t.ink2 }]} numberOfLines={2}>{label}</Text>
          </View>
        )}
      </>
    );
    if (!onPressSlot) return <View style={[styles.cell, { flex }]}>{inner}</View>;
    return (
      <Pressable
        onPress={() => onPressSlot(slot)}
        accessibilityRole="button"
        accessibilityLabel={`Swap ${SLOT_LABEL[slot].toLowerCase()}: ${label}`}
        style={({ pressed }) => [styles.cell, { flex }, pressed && { backgroundColor: t.tile2 }]}
      >
        {inner}
      </Pressable>
    );
  };
  const one = (i?: Item) => (i ? [i] : []);
  return (
    <View style={[styles.board, { backgroundColor: t.tile }, style]}>
      <View style={{ flex: 1.12 }}>{cell('outer', one(pieces.outer), 1)}</View>
      <View style={{ flex: 1, gap: 4 }}>{cell('top', one(pieces.top), 1)}{cell('bottom', one(pieces.bottom), 1)}</View>
      <View style={{ flex: 0.78, gap: 4 }}>{cell('acc', pieces.acc, 1)}{cell('shoes', one(pieces.shoes), 1)}</View>
    </View>
  );
}

export function MiniBoard({ pieces, style }: { pieces: Resolved; style?: ViewStyle }) {
  const t = useTheme();
  const list = [pieces.outer, pieces.top, pieces.bottom, pieces.shoes];
  return (
    <View style={[styles.mini, { backgroundColor: t.tile }, style]}>
      {list.map((i, k) => <View key={k} style={styles.miniCell}>{i && <Garment item={i} />}</View>)}
    </View>
  );
}

export function PieceRow({ pieces }: { pieces: Resolved }) {
  const t = useTheme();
  const list = [pieces.outer, pieces.top, pieces.bottom, pieces.shoes];
  return (
    <View style={{ flexDirection: 'row', gap: 4, flex: 1 }}>
      {list.map((i, k) => (
        <View key={k} style={{ flex: 1, aspectRatio: 1, backgroundColor: t.tile, padding: 6, borderRadius: 2 }}>
          {i && <Garment item={i} />}
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  board: { flexDirection: 'row', aspectRatio: 1 / 1.02, padding: 10, gap: 4, borderRadius: 3 },
  cell: { alignItems: 'center', justifyContent: 'center', borderRadius: 2, padding: 2 },
  cap: { alignSelf: 'stretch', paddingHorizontal: 2, paddingTop: 4, minHeight: 34 },
  capSlot: { fontFamily: fonts.monoMedium, fontSize: 9, letterSpacing: 0.6, textTransform: 'uppercase' },
  capName: { fontFamily: fonts.mono, fontSize: 9, letterSpacing: 0.4, textTransform: 'uppercase', lineHeight: 11 },
  mini: { flexDirection: 'row', flexWrap: 'wrap', aspectRatio: 1 / 1.05, padding: 6, borderRadius: 3 },
  miniCell: { width: '50%', height: '50%', padding: 2 },
});
