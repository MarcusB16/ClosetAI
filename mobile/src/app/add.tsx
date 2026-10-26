import { router } from 'expo-router';
import { useEffect, useState } from 'react';
import { Animated, Image, Pressable, StyleSheet, TextInput, View } from 'react-native';

import { api } from '@/api';
import { errorMessage } from '@/api/errors';
import { useAddItem } from '@/api/hooks';
import { useAttempt } from '@/api/useAttempt';
import { Icon } from '@/components/Icon';
import { useToast } from '@/components/Toast';
import { Btn, Chip, IconBtn, Row, Screen, T } from '@/components/ui';
import { pickPhoto } from '@/domain/photos';
import { CATEGORY_LABEL, type Category, type GarmentType, type ItemSuggestion, type Slot } from '@/domain/types';
import { fonts, gutter, useTheme } from '@/theme';

const DEFAULTS: Record<Category, { slot: Slot; type: GarmentType }> = {
  shirts: { slot: 'top', type: 'shirt' },
  pants: { slot: 'bottom', type: 'pants' },
  jackets: { slot: 'outer', type: 'jacket' },
  shoes: { slot: 'shoes', type: 'sneaker' },
  accessories: { slot: 'acc', type: 'tote' },
  formal: { slot: 'top', type: 'shirt' },
};
const LEVELS = [['Casual', 1], ['Smart casual', 2], ['Formal', 3]] as const;
const SEASONS = ['Spring', 'Summer', 'Fall', 'Winter'];

type Step = 'capture' | 'analyzing' | 'details';

export default function AddItem() {
  const t = useTheme();
  const toast = useToast();
  const addItem = useAddItem();
  const attempt = useAttempt();
  const [step, setStep] = useState<Step>('capture');
  const [photo, setPhoto] = useState<string>();
  const [s, setS] = useState<ItemSuggestion>();
  const [form, setForm] = useState({ name: '', brand: '', size: '', colorName: '', category: 'shirts' as Category, formality: 2, seasons: [] as string[] });
  const set = <K extends keyof typeof form>(k: K, v: (typeof form)[K]) => setForm(f => ({ ...f, [k]: v }));

  const take = async (source: 'camera' | 'library') => {
    const uri = await pickPhoto(source);
    if (!uri) return;
    setPhoto(uri);
    setStep('analyzing');
    try {
      const sug = await api.analyzePhoto(uri);
      setS(sug);
      setForm(f => ({ ...f, name: sug.name, brand: sug.brand ?? '', colorName: sug.colorName, category: sug.category, formality: sug.formality, seasons: sug.seasons }));
      setStep('details');
    } catch (error) {
      toast(errorMessage(error, "Couldn't read that photo. Try again in better light."));
      setStep('capture');
    }
  };

  const save = async () => {
    if (!s) return;
    const d = form.category === s.category ? { slot: s.slot ?? DEFAULTS[form.category].slot, type: s.type ?? DEFAULTS[form.category].type } : DEFAULTS[form.category];
    const name = form.name.trim() || `New ${CATEGORY_LABEL[form.category].toLowerCase().replace(/s$/, '')}`;
    const added = await attempt(() => addItem.mutateAsync({
      name, brand: form.brand.trim() || 'Unknown', size: form.size.trim() || '—',
      category: form.category, slot: d.slot, type: d.type, imageUri: s.cutoutUri,
      color: s.color, colorName: form.colorName.trim() || 'Unknown', formality: form.formality,
      fabric: s.fabric ?? 'Not set', seasons: form.seasons,
    }), "Couldn't add this piece. Try again.");
    if (!added) return;
    toast(`${name} is in your closet`);
    router.back();
    router.navigate('/closet');
  };

  if (step !== 'details') {
    return (
      <Screen contentStyle={{ paddingTop: 14 }}>
        <View style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' }}>
          <IconBtn icon="x" label="Close" onPress={() => router.back()} />
          <T variant="strong">Add a piece</T>
          <View style={{ width: 40 }} />
        </View>
        <View style={[styles.finder, { marginHorizontal: -gutter, backgroundColor: step === 'analyzing' ? t.tile : '#5E584F' }]}>
          {photo ? <Image source={{ uri: photo }} style={styles.photo} resizeMode="contain" /> : (
            <View style={{ alignItems: 'center', gap: 10 }}>
              <Icon name="camera" color="#FFFFFF" size={36} />
              <T style={{ color: '#FFFFFF', textAlign: 'center' }}>Lay it flat or hang it up.{'\n'}Fill the frame.</T>
            </View>
          )}
          {step === 'capture' && ['c1', 'c2', 'c3', 'c4'].map(c => <View key={c} style={[styles.corner, styles[c as 'c1']]} />)}
          {step === 'analyzing' && <ScanLine />}
        </View>
        {step === 'capture' ? (
          <>
            <View style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: 12 }}>
              <IconBtn icon="photos" label="Choose from photos" onPress={() => take('library')} />
              <Pressable onPress={() => take('camera')} accessibilityRole="button" accessibilityLabel="Take photo"
                style={[styles.shutter, { borderColor: t.ink }]}>
                <View style={[styles.shutterIn, { backgroundColor: t.ink }]} />
              </Pressable>
              <View style={{ width: 40 }} />
            </View>
            <T tone="muted" style={{ textAlign: 'center' }}>One photo is enough. Closet removes the background and fills in the details.</T>
          </>
        ) : (
          <T variant="displaySm" style={{ textAlign: 'center', fontStyle: 'italic' }}>Removing the background…</T>
        )}
      </Screen>
    );
  }

  return (
    <Screen contentStyle={{ paddingTop: 14 }}>
      <View style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' }}>
        <IconBtn icon="back" label="Retake" onPress={() => setStep('capture')} />
        <T variant="strong">Check the details</T>
        <View style={{ width: 40 }} />
      </View>
      <View style={{ backgroundColor: t.tile, aspectRatio: 1.25, padding: 22, borderRadius: 3 }}>
        {s && <Image source={{ uri: s.cutoutUri }} style={styles.photo} resizeMode="contain" />}
      </View>
      <Field label="Name" value={form.name} onChange={v => set('name', v)} placeholder="White oxford shirt" />
      <View style={{ flexDirection: 'row', gap: 14 }}>
        <View style={{ flex: 1 }}><Field label="Brand" value={form.brand} onChange={v => set('brand', v)} placeholder="J.Crew" /></View>
        <View style={{ flex: 1 }}><Field label="Size" value={form.size} onChange={v => set('size', v)} placeholder="M" /></View>
      </View>
      <Field label="Color" value={form.colorName} onChange={v => set('colorName', v)} placeholder="Navy" />
      <Group label="Category">
        {(Object.keys(CATEGORY_LABEL) as Category[]).map(c => <Chip key={c} label={CATEGORY_LABEL[c]} selected={form.category === c} onPress={() => set('category', c)} />)}
      </Group>
      <Group label="When you'd wear it">
        {LEVELS.map(([l, v]) => <Chip key={l} label={l} selected={form.formality === v} onPress={() => set('formality', v)} />)}
      </Group>
      <Group label="Seasons">
        {SEASONS.map(x => (
          <Chip key={x} label={x} selected={form.seasons.includes(x)}
            onPress={() => set('seasons', form.seasons.includes(x) ? form.seasons.filter(y => y !== x) : [...form.seasons, x])} />
        ))}
      </Group>
      <Btn label="Add to closet" loading={addItem.isPending} onPress={save} />
    </Screen>
  );
}

function Field({ label, value, onChange, placeholder }: { label: string; value: string; onChange: (v: string) => void; placeholder?: string }) {
  const t = useTheme();
  return (
    <View style={{ gap: 4 }}>
      <T variant="small" tone="muted">{label}</T>
      <TextInput value={value} onChangeText={onChange} placeholder={placeholder} placeholderTextColor={t.ink2} accessibilityLabel={label}
        style={{ borderBottomWidth: 1, borderColor: t.line, paddingVertical: 8, color: t.ink, fontFamily: fonts.body, fontSize: 16 }} />
    </View>
  );
}

function Group({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <View style={{ gap: 8 }}>
      <T variant="small" tone="muted">{label}</T>
      <Row gap={8}>{children}</Row>
    </View>
  );
}

function ScanLine() {
  const t = useTheme();
  const [y] = useState(() => new Animated.Value(0));
  useEffect(() => {
    const loop = Animated.loop(Animated.sequence([
      Animated.timing(y, { toValue: 1, duration: 1100, useNativeDriver: false }),
      Animated.timing(y, { toValue: 0, duration: 1100, useNativeDriver: false }),
    ]));
    loop.start();
    return () => loop.stop();
  }, [y]);
  return <Animated.View style={{ position: 'absolute', left: 0, right: 0, height: 2, backgroundColor: t.accent, top: y.interpolate({ inputRange: [0, 1], outputRange: ['12%', '88%'] }) }} />;
}

const styles = StyleSheet.create({
  finder: { aspectRatio: 1 / 1.1, alignItems: 'center', justifyContent: 'center', padding: 40, overflow: 'hidden' },
  photo: { width: '100%', height: '100%' },
  corner: { position: 'absolute', width: 28, height: 28, borderColor: '#FFFFFF' },
  c1: { top: 22, left: 22, borderTopWidth: 2, borderLeftWidth: 2 },
  c2: { top: 22, right: 22, borderTopWidth: 2, borderRightWidth: 2 },
  c3: { bottom: 22, left: 22, borderBottomWidth: 2, borderLeftWidth: 2 },
  c4: { bottom: 22, right: 22, borderBottomWidth: 2, borderRightWidth: 2 },
  shutter: { width: 72, height: 72, borderRadius: 36, borderWidth: 3, alignItems: 'center', justifyContent: 'center' },
  shutterIn: { width: 58, height: 58, borderRadius: 29 },
});
