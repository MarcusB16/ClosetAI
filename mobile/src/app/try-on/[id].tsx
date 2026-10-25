import { router, useLocalSearchParams } from 'expo-router';
import { useEffect, useState } from 'react';
import { AccessibilityInfo, Animated, Image, StyleSheet, View } from 'react-native';

import { useItemMap, useOutfit, useProfile, useTryOn } from '@/api/hooks';
import { MiniBoard, resolvePieces } from '@/components/Board';
import { Icon } from '@/components/Icon';
import { Btn, IconBtn, Loading, Screen, T } from '@/components/ui';
import type { Item } from '@/domain/types';
import { useTheme } from '@/theme';

/** "See it on me": calls the image model with the saved photo and this outfit, then shows the result. */
export default function TryOn() {
  const t = useTheme();
  const { id } = useLocalSearchParams<{ id: string }>();
  const { data: outfit } = useOutfit(id);
  const { data: profile } = useProfile();
  const itemMap = useItemMap();
  const tryOn = useTryOn(outfit);

  if (!outfit || !profile) return <Loading />;
  const pieces = resolvePieces(outfit, itemMap);
  const names = [pieces.outer, pieces.top, pieces.bottom, pieces.shoes].filter(Boolean) as Item[];
  const result = tryOn.data;

  const header = (
    <View style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' }}>
      <IconBtn icon="x" label="Close" onPress={() => router.back()} />
      <T variant="strong">On you</T>
      <View style={{ width: 40 }} />
    </View>
  );

  if (result?.status === 'needs-photo') {
    return (
      <Screen contentStyle={{ paddingTop: 14 }}>
        {header}
        <View style={{ backgroundColor: t.tile, aspectRatio: 3 / 4, alignItems: 'center', justifyContent: 'center', gap: 12, padding: 24, borderRadius: 3 }}>
          <Icon name="person" color={t.ink2} size={48} />
          <T variant="displaySm" style={{ textAlign: 'center' }}>Add your photo first</T>
          <T tone="muted" style={{ textAlign: 'center' }}>One full-body photo lets Closet show every outfit on you.</T>
        </View>
        <Btn label="Add my photo" icon="camera" onPress={() => router.replace('/setup?edit=1')} />
      </Screen>
    );
  }

  const failed = tryOn.isError || result?.status === 'failed';

  return (
    <Screen contentStyle={{ paddingTop: 14 }}>
      {header}
      <T variant="display">{outfit.title}</T>

      <View style={[styles.stage, { backgroundColor: t.tile }]}>
        {result?.status === 'ready' && result.imageUri ? (
          <Image source={{ uri: result.imageUri }} style={StyleSheet.absoluteFill} resizeMode="cover" accessibilityLabel={`You wearing ${outfit.title}`} />
        ) : failed ? (
          <View style={{ alignItems: 'center', gap: 12, padding: 24 }}>
            <T variant="displaySm" style={{ textAlign: 'center' }}>Couldn’t make your preview</T>
            <T tone="muted" style={{ textAlign: 'center' }}>{result?.message ?? 'Check your connection and try again.'}</T>
          </View>
        ) : (
          <Dressing photo={profile.personPhotoUri} pieces={names} />
        )}
        {result?.placeholder && (
          <View style={[styles.flag, { backgroundColor: t.paper }]}>
            <T variant="label" tone="ink">Placeholder until the image service is connected</T>
          </View>
        )}
      </View>

      {failed && <Btn label="Try again" icon="again" onPress={() => tryOn.refetch()} />}

      <View style={{ flexDirection: 'row', gap: 12, alignItems: 'center' }}>
        <View style={{ width: 96 }}><MiniBoard pieces={pieces} /></View>
        <T variant="small" tone="muted" style={{ flex: 1 }}>{names.map(i => i.name).join(', ')}</T>
      </View>
    </Screen>
  );
}

/** Loading state while the image model works: the user's photo, dimmed, with a sweep and the piece being "put on". */
function Dressing({ photo, pieces }: { photo?: string; pieces: Item[] }) {
  const t = useTheme();
  const [sweep] = useState(() => new Animated.Value(0));
  const [step, setStep] = useState(0);

  useEffect(() => {
    let loop: Animated.CompositeAnimation | undefined;
    AccessibilityInfo.isReduceMotionEnabled().then(reduce => {
      if (reduce) return;
      loop = Animated.loop(Animated.timing(sweep, { toValue: 1, duration: 1600, useNativeDriver: false }));
      loop.start();
    });
    const timer = setInterval(() => setStep(s => s + 1), 900);
    return () => { loop?.stop(); clearInterval(timer); };
  }, [sweep]);

  const piece = pieces.length ? pieces[step % pieces.length] : undefined;

  return (
    <View style={[StyleSheet.absoluteFill, { alignItems: 'center', justifyContent: 'flex-end' }]} accessibilityLiveRegion="polite">
      {photo && <Image source={{ uri: photo }} style={[StyleSheet.absoluteFill, { opacity: 0.35 }]} resizeMode="cover" blurRadius={6} />}
      <Animated.View style={{
        position: 'absolute', left: 0, right: 0, height: 2, backgroundColor: t.accent,
        top: sweep.interpolate({ inputRange: [0, 1], outputRange: ['5%', '95%'] }),
      }} />
      <View style={[styles.caption, { backgroundColor: t.paper }]}>
        <T variant="displaySm" style={{ fontStyle: 'italic' }}>Dressing you…</T>
        {piece && <T variant="label">Adding the {piece.name.toLowerCase()}</T>}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  stage: { aspectRatio: 3 / 4, borderRadius: 3, overflow: 'hidden', alignItems: 'center', justifyContent: 'center' },
  caption: { margin: 14, paddingVertical: 12, paddingHorizontal: 16, borderRadius: 3, alignSelf: 'stretch', gap: 4 },
  flag: { position: 'absolute', top: 10, left: 10, right: 10, paddingVertical: 6, paddingHorizontal: 10, borderRadius: 2 },
});
