import { router } from 'expo-router';
import { useState } from 'react';
import { TextInput, View } from 'react-native';

import { useItems } from '@/api/hooks';
import { Icon } from '@/components/Icon';
import { ItemTile } from '@/components/ItemTile';
import { Chip, HScroll, Screen, T } from '@/components/ui';
import { daysSince, fmtMonth } from '@/domain/dates';
import { CATEGORY_LABEL, type Category } from '@/domain/types';
import { fonts, useTheme } from '@/theme';

const CATS: ('all' | Category)[] = ['all', 'shirts', 'pants', 'jackets', 'shoes', 'accessories', 'formal'];

export default function Closet() {
  const t = useTheme();
  const { data: items = [] } = useItems();
  const [cat, setCat] = useState<'all' | Category>('all');
  const [q, setQ] = useState('');

  const query = q.trim().toLowerCase();
  const list = items.filter(i =>
    (cat === 'all' || i.category === cat) &&
    (!query || `${i.name} ${i.brand} ${i.colorName}`.toLowerCase().includes(query)));

  const badge = (i: (typeof items)[number]) => {
    if (daysSince(i.addedAt.slice(0, 10)) <= 2 && i.timesWorn === 0) return { text: 'New', accent: true };
    if (i.lastWorn && daysSince(i.lastWorn) > 90) return { text: `Not worn since ${fmtMonth(i.lastWorn)}`, accent: false };
    return undefined;
  };

  return (
    <Screen>
      <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'baseline' }}>
        <T variant="display">Closet</T>
        <T variant="label">{items.length} pieces</T>
      </View>
      <View style={{ flexDirection: 'row', alignItems: 'center', gap: 8, backgroundColor: t.tile, borderRadius: 999, paddingHorizontal: 14 }}>
        <Icon name="search" color={t.ink2} />
        <TextInput value={q} onChangeText={setQ} placeholder="Search your closet" placeholderTextColor={t.ink2}
          accessibilityLabel="Search your closet" autoCorrect={false}
          style={{ flex: 1, paddingVertical: 11, color: t.ink, fontFamily: fonts.body, fontSize: 15 }} />
      </View>
      <HScroll gap={8}>
        {CATS.map(c => <Chip key={c} label={c === 'all' ? 'All' : CATEGORY_LABEL[c]} selected={cat === c} onPress={() => setCat(c)} />)}
      </HScroll>
      <View style={{ flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between', rowGap: 16 }}>
        {list.length === 0 && <T tone="muted">Nothing matches “{q}”. Try a color or a brand.</T>}
        {list.map(i => {
          const b = badge(i);
          return (
            <ItemTile key={i.id} item={i} sub={i.brand} badge={b?.text} badgeAccent={b?.accent}
              style={{ width: '48.5%' }} onPress={() => router.push(`/item/${i.id}`)} />
          );
        })}
      </View>
    </Screen>
  );
}
