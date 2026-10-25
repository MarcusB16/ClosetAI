import * as Haptics from 'expo-haptics';
import { router, useLocalSearchParams } from 'expo-router';
import { useState } from 'react';
import { View } from 'react-native';

import {
  useItemMap, useLogWear, useOutfit, useRefineOutfit, useRejectOutfit, useReplacePiece, useSetSaved,
} from '@/api/hooks';
import { Board, resolvePieces } from '@/components/Board';
import { SwapSheet } from '@/components/SwapSheet';
import { useToast } from '@/components/Toast';
import { Btn, IconBtn, Loading, PillInput, RoundBtn, Screen, T } from '@/components/ui';
import { slotFromInstruction } from '@/domain/slots';
import { SLOT_LABEL, type Slot } from '@/domain/types';

export default function OutfitScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const [outfitId, setOutfitId] = useState(id);
  const { data: outfit } = useOutfit(outfitId);
  const itemMap = useItemMap();
  const toast = useToast();
  const [slot, setSlot] = useState<Slot | null>(null);
  const [refine, setRefine] = useState('');

  const replace = useReplacePiece();
  const refineM = useRefineOutfit();
  const reject = useRejectOutfit();
  const saved = useSetSaved();
  const wear = useLogWear();

  if (!outfit || itemMap.size === 0) return <Loading />;
  const pieces = resolvePieces(outfit, itemMap);
  const current = slot ? (slot === 'acc' ? pieces.acc[1] : pieces[slot]) : undefined;

  const toggleSave = async () => {
    await saved.mutateAsync({ id: outfit.id, saved: !outfit.saved });
    Haptics.selectionAsync();
    toast(outfit.saved ? 'Removed from saved' : 'Saved to Outfits');
  };

  const another = async (rejected: boolean) => {
    const next = await reject.mutateAsync(outfit.id);
    setOutfitId(next.id);
    if (rejected) toast('Got it. Fewer outfits like that one.');
  };

  const applyRefine = async () => {
    const text = refine.trim();
    if (!text) return;
    const next = await refineM.mutateAsync({ id: outfit.id, instruction: text });
    setRefine('');
    if (next.id !== outfit.id) setOutfitId(next.id);
    const s = slotFromInstruction(text);
    toast(s ? `New ${SLOT_LABEL[s].toLowerCase()}. Everything else stays.` : 'Restyled with your note.');
  };

  return (
    <View style={{ flex: 1 }}>
      <Screen>
        <View style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' }}>
          <IconBtn icon="back" label="Back" onPress={() => router.back()} />
          <T variant="label">Tap any piece to swap it</T>
          <IconBtn icon="heart" label={outfit.saved ? 'Unsave outfit' : 'Save outfit'} active={outfit.saved} onPress={toggleSave} />
        </View>
        <View style={{ gap: 6 }}>
          {outfit.prompt && <T variant="quote" tone="muted">“{outfit.prompt}”</T>}
          <T variant="display">{outfit.title}</T>
        </View>
        <Board pieces={pieces} captions onPressSlot={setSlot} style={{ opacity: reject.isPending ? 0.4 : 1 }} />
        {outfit.note && <T>{outfit.note}</T>}
        <PillInput value={refine} onChangeText={setRefine} onSubmit={applyRefine}
          placeholder="Change one thing, like “different shoes”" accessibilityLabel="Change one thing" />
        <View style={{ flexDirection: 'row', alignItems: 'center', gap: 8 }}>
          <RoundBtn icon="x" label="Not for me" onPress={() => another(true)} />
          <RoundBtn icon="again" label="Show another outfit" onPress={() => another(false)} />
          <RoundBtn icon="heart" label="Save" active={outfit.saved} onPress={toggleSave} />
          <Btn label="Wear this" icon="check" style={{ flex: 1 }} loading={wear.isPending} onPress={async () => {
            await wear.mutateAsync(outfit.id);
            Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
            toast("Logged for today. It's in Outfits.");
          }} />
        </View>
        <Btn label="See it on me" icon="person" variant="ghost" onPress={() => router.push(`/try-on/${outfit.id}`)} />
      </Screen>
      <SwapSheet
        outfitId={outfit.id}
        slot={slot}
        current={current}
        onClose={() => setSlot(null)}
        onPick={async item => {
          const s = slot!;
          setSlot(null);
          await replace.mutateAsync({ id: outfit.id, slot: s, itemId: item.id });
          Haptics.selectionAsync();
        }}
      />
    </View>
  );
}
