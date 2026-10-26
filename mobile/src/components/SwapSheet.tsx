import { Modal, Pressable, StyleSheet, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { useAlternatives } from '@/api/hooks';
import { SLOT_LABEL, type Item, type Slot } from '@/domain/types';
import { useTheme } from '@/theme';
import { ItemTile } from './ItemTile';
import { Btn, ErrorState, HScroll, SectionHead, T } from './ui';

export function SwapSheet({ outfitId, slot, current, onPick, onClose }: {
  outfitId: string; slot: Slot | null; current?: Item; onPick: (item: Item) => void; onClose: () => void;
}) {
  const t = useTheme();
  const insets = useSafeAreaInsets();
  const { data: alts, isLoading, isError, refetch } = useAlternatives(outfitId, slot);
  const label = slot ? SLOT_LABEL[slot].toLowerCase() : '';

  return (
    <Modal visible={!!slot} transparent animationType="slide" onRequestClose={onClose}>
      <Pressable style={[StyleSheet.absoluteFill, { backgroundColor: t.scrim }]} onPress={onClose} accessibilityLabel="Close" />
      <View style={[styles.sheet, { backgroundColor: t.paper, paddingBottom: insets.bottom + 18 }]}>
        <View style={[styles.grab, { backgroundColor: t.line }]} />
        <SectionHead title={`Swap the ${label}`} right={alts && <T variant="label">{alts.length} that work</T>} />
        {isError ? <ErrorState message="Couldn't load options." onRetry={() => refetch()} /> : isLoading ? <T tone="muted">Finding options…</T> : (
          <HScroll>
            {current && <ItemTile item={current} width={104} selected onPress={onClose} />}
            {alts?.map(i => <ItemTile key={i.id} item={i} width={104} onPress={() => onPick(i)} />)}
          </HScroll>
        )}
        {alts?.length === 0 && <T tone="muted">Nothing else in your closet fits this look.</T>}
        <Btn variant="ghost" label={current ? `Keep the ${current.name.toLowerCase()}` : 'Keep it'} onPress={onClose} />
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  sheet: { position: 'absolute', left: 0, right: 0, bottom: 0, borderTopLeftRadius: 20, borderTopRightRadius: 20, paddingHorizontal: 18, paddingTop: 10, gap: 16 },
  grab: { width: 38, height: 4, borderRadius: 2, alignSelf: 'center' },
});
