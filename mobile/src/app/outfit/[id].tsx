import * as Haptics from 'expo-haptics';
import { router, useLocalSearchParams } from 'expo-router';
import { useState } from 'react';
import { View } from 'react-native';

import {
  useItemMap, useLogWear, useOutfit, useRefineOutfit, useRejectOutfit, useReplacePiece, useSetSaved,
} from '@/api/hooks';
import { useAttempt } from '@/api/useAttempt';
import { Board, resolvePieces } from '@/components/Board';
import { SwapSheet } from '@/components/SwapSheet';
import { useToast } from '@/components/Toast';
import { Btn, ErrorScreen, IconBtn, Loading, PillInput, RoundBtn, Screen, T } from '@/components/ui';
import { slotFromInstruction } from '@/domain/slots';
import { SLOT_LABEL, type Slot } from '@/domain/types';

export default function OutfitScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const [outfitId, setOutfitId] = useState(id);
  const { data: outfit, isError, refetch } = useOutfit(outfitId);
  const itemMap = useItemMap();
  const attempt = useAttempt();
  const toast = useToast();
  const [slot, setSlot] = useState<Slot | null>(null);
  const [refine, setRefine] = useState('');

  const replace = useReplacePiece();
  const refineM = useRefineOutfit();
  const reject = useRejectOutfit();
  const saved = useSetSaved();
  const wear = useLogWear();

  if (isError) return <ErrorScreen message="Couldn't load this outfit." onRetry={() => refetch()} />;
  if (!outfit || itemMap.size === 0) return <Loading />;
  const pieces = resolvePieces(outfit, itemMap);
  const current = slot ? (slot === 'acc' ? pieces.acc[1] : pieces[slot]) : undefined;

  const toggleSave = async () => {
    const result = await attempt(() => saved.mutateAsync({ id: outfit.id, saved: !outfit.saved }), "Couldn't update your saved outfits. Try again.");
    if (!result) return;
    Haptics.selectionAsync();
    toast(outfit.saved ? 'Removed from saved' : 'Saved to Outfits');
  };

  const another = async (rejected: boolean) => {
    if (reject.isPending) return;
    const next = await attempt(() => reject.mutateAsync(outfit.id), "Couldn't find another outfit. Try again.");
    if (!next) return;
    setOutfitId(next.id);
    if (rejected) toast('Got it. Fewer outfits like that one.');
  };

  const logWear = async () => {
    const entry = await attempt(() => wear.mutateAsync(outfit.id), "Couldn't log this outfit. Try again.");
    if (!entry) return;
    Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
    toast("Logged for today. It's in Outfits.");
  };

  const applyRefine = async () => {
    const text = refine.trim();
    if (!text || refineM.isPending) return;
    const next = await attempt(() => refineM.mutateAsync({ id: outfit.id, instruction: text }), "Couldn't change the outfit. Try again.");
    if (!next) return;
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
        <Board pieces={pieces} captions onPressSlot={setSlot} style={{ opacity: reject.isPending || refineM.isPending ? 0.4 : 1 }} />
        {outfit.note && <T>{outfit.note}</T>}
        <PillInput value={refine} onChangeText={setRefine} onSubmit={applyRefine}
          placeholder="Change one thing, like “different shoes”" accessibilityLabel="Change one thing"
          editable={!refineM.isPending} />
        {(refineM.isPending || reject.isPending) && <T variant="label" accessibilityLiveRegion="polite">{refineM.isPending ? 'Restyling…' : 'Finding another outfit…'}</T>}
        <View style={{ flexDirection: 'row', alignItems: 'center', gap: 8 }}>
          <RoundBtn icon="x" label="Not for me" onPress={() => another(true)} />
          <RoundBtn icon="again" label="Show another outfit" onPress={() => another(false)} />
          <RoundBtn icon="heart" label="Save" active={outfit.saved} onPress={toggleSave} />
          <Btn label="Wear this" icon="check" style={{ flex: 1 }} loading={wear.isPending} onPress={logWear} />
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
          const next = await attempt(() => replace.mutateAsync({ id: outfit.id, slot: s, itemId: item.id }), "Couldn't swap that piece. Try again.");
          if (next) Haptics.selectionAsync();
        }}
      />
    </View>
  );
}
