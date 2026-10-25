import * as Haptics from 'expo-haptics';
import { router } from 'expo-router';
import { useState } from 'react';
import { Image, Pressable, View } from 'react-native';

import { useContext, useItemMap, useItems, useLogWear, useProfile, useTodaysOutfit } from '@/api/hooks';
import { Board, resolvePieces } from '@/components/Board';
import { ItemTile } from '@/components/ItemTile';
import { useToast } from '@/components/Toast';
import { Btn, Chip, HScroll, PillInput, Row, Screen, SectionHead, T } from '@/components/ui';
import { daysSince, fmtMonth } from '@/domain/dates';
import { startStyling } from '@/domain/navigation';
import { PROMPTS, QUICK } from '@/domain/prompts';
import { fonts, useTheme } from '@/theme';

export default function Home() {
  const t = useTheme();
  const toast = useToast();
  const [ask, setAsk] = useState('');
  const { data: ctx } = useContext();
  const { data: items } = useItems();
  const itemMap = useItemMap();
  const { data: ootd } = useTodaysOutfit();
  const wear = useLogWear();
  const { data: profile } = useProfile();

  const forgotten = (items ?? [])
    .filter(i => daysSince(i.lastWorn) > 120 && !(ctx?.rain && i.noRain))
    .sort((a, b) => (a.lastWorn ?? '').localeCompare(b.lastWorn ?? ''))
    .slice(0, 6);

  const pieces = ootd ? resolvePieces(ootd, itemMap) : undefined;
  const pieceText = pieces
    ? [pieces.outer, pieces.top, pieces.bottom, pieces.shoes].filter(Boolean).map(i => i!.name.toLowerCase()).join(', ').replace(/^./, c => c.toUpperCase())
    : '';

  const submit = () => {
    startStyling(ask.trim() || PROMPTS[0]);
    setAsk('');
  };

  return (
    <Screen>
      <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' }}>
        <T style={{ fontFamily: fonts.displayItalic, fontSize: 26 }}>Closet</T>
        <Pressable onPress={() => router.push('/setup?edit=1')} accessibilityRole="button" accessibilityLabel="Your try-on photo"
          style={{ width: 34, height: 34, borderRadius: 17, backgroundColor: t.tile2, alignItems: 'center', justifyContent: 'center', overflow: 'hidden' }}>
          {profile?.personPhotoUri
            ? <Image source={{ uri: profile.personPhotoUri }} style={{ width: 34, height: 34 }} />
            : <T variant="strong" style={{ fontSize: 13 }}>MB</T>}
        </Pressable>
      </View>

      <View style={{ gap: 14 }}>
        {ctx && <T variant="label">{ctx.weekday} · {ctx.tempF}° · {ctx.forecast}</T>}
        <T variant="hero">
          What should I <T variant="hero" style={{ fontFamily: fonts.displayItalic }}>wear?</T>
        </T>
        <PillInput
          underline
          value={ask}
          onChangeText={setAsk}
          onSubmit={submit}
          placeholder="Dinner tonight, a client meeting…"
          accessibilityLabel="Describe what you need"
        />
        <HScroll gap={8}>
          {QUICK.map(q => <Chip key={q.label} label={q.label} onPress={() => startStyling(q.prompt)} />)}
        </HScroll>
      </View>

      {ootd && pieces && (
        <View style={{ gap: 14 }}>
          <SectionHead title="Outfit of the day" right={ctx?.rain ? <T variant="label">Picked for rain</T> : undefined} />
          <Pressable onPress={() => router.push(`/outfit/${ootd.id}`)} accessibilityRole="button" accessibilityLabel="Open outfit of the day">
            <Board pieces={pieces} />
          </Pressable>
          <T variant="small" tone="muted">{pieceText}</T>
          <Row>
            <Btn label="Wear this" icon="check" loading={wear.isPending} onPress={async () => {
              await wear.mutateAsync(ootd.id);
              Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
              toast("Logged for today. It's in Outfits.");
            }} />
            <Btn label="See it on me" icon="person" variant="ghost" onPress={() => router.push(`/try-on/${ootd.id}`)} />
            <Btn label="Change something" variant="ghost" onPress={() => router.push(`/outfit/${ootd.id}`)} />
          </Row>
        </View>
      )}

      {forgotten.length > 0 && (
        <View style={{ gap: 14 }}>
          <SectionHead title="Not worn in a while" right={<Pressable onPress={() => router.navigate('/closet')}><T variant="label">See closet</T></Pressable>} />
          <HScroll>
            {forgotten.map(i => (
              <ItemTile key={i.id} item={i} width={112} sub={i.lastWorn ? `Last worn ${fmtMonth(i.lastWorn)}` : 'Never worn'}
                onPress={() => router.push(`/item/${i.id}`)} />
            ))}
          </HScroll>
        </View>
      )}
    </Screen>
  );
}
