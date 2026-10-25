import { router, useLocalSearchParams } from 'expo-router';
import { useEffect, useRef, useState } from 'react';
import { Pressable, TextInput, View } from 'react-native';

import { useContext, useItems, useStyleOutfit } from '@/api/hooks';
import { Icon } from '@/components/Icon';
import { StylingOverlay } from '@/components/StylingOverlay';
import { useToast } from '@/components/Toast';
import { Btn, Chip, Row, Screen, T } from '@/components/ui';
import { PROMPTS } from '@/domain/prompts';
import { fonts, useTheme } from '@/theme';

export default function StyleRequest() {
  const t = useTheme();
  const toast = useToast();
  const params = useLocalSearchParams<{ prompt?: string; lock?: string; n?: string }>();
  const [prompt, setPrompt] = useState('');
  const [avoid, setAvoid] = useState(false);
  const { data: ctx } = useContext();
  const { data: items = [] } = useItems();
  const styleOutfit = useStyleOutfit();
  const handled = useRef<string | undefined>(undefined);

  const run = async (text: string, lock?: string) => {
    const p = text.trim() || PROMPTS[0];
    setPrompt(p);
    try {
      const outfit = await styleOutfit.mutateAsync({ prompt: p, lockItemId: lock || undefined, avoidRecent: avoid });
      router.push(`/outfit/${outfit.id}`);
    } catch {
      toast("Couldn't build an outfit. Check your connection and try again.");
    }
  };

  // Requests started from Home, a chip or an item page arrive as params.
  useEffect(() => {
    if (params.n && params.n !== handled.current && params.prompt) {
      handled.current = params.n;
      run(params.prompt, params.lock);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params.n]);

  return (
    <View style={{ flex: 1 }}>
      <Screen contentStyle={{ flexGrow: 1 }}>
        <T variant="label">Ask your stylist</T>
        <TextInput
          value={prompt}
          onChangeText={setPrompt}
          multiline
          placeholder="What's the occasion?"
          placeholderTextColor={t.ink2}
          accessibilityLabel="What you need"
          style={{ fontFamily: fonts.display, fontSize: 30, lineHeight: 34, color: t.ink, minHeight: 130, textAlignVertical: 'top' }}
        />
        <Row gap={8}>
          {ctx && <Chip icon="rain" label={`${ctx.tempF}° · ${ctx.forecast}`} selected />}
          {ctx && <Chip icon="cal" label={ctx.weekday} selected />}
          <Chip label="Skip what I wore this week" selected={avoid} onPress={() => setAvoid(a => !a)} />
        </Row>
        <View style={{ gap: 8 }}>
          <T variant="label">Or try</T>
          <View style={{ borderTopWidth: 1, borderColor: t.line }}>
            {PROMPTS.map(p => (
              <Pressable key={p} onPress={() => setPrompt(p)} accessibilityRole="button"
                style={({ pressed }) => ({ paddingVertical: 13, borderBottomWidth: 1, borderColor: t.line, flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', gap: 12, opacity: pressed ? 0.6 : 1 })}>
                <T style={{ flex: 1 }}>{p}</T>
                <Icon name="arrow" color={t.ink2} size={18} />
              </Pressable>
            ))}
          </View>
        </View>
        <Btn label="Style me" icon="spark" onPress={() => run(prompt)} />
      </Screen>
      {styleOutfit.isPending && <StylingOverlay items={items} prompt={prompt} />}
    </View>
  );
}
