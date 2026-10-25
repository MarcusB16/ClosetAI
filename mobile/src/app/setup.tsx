import { router, useLocalSearchParams } from 'expo-router';
import { useState } from 'react';
import { Image, Pressable, View } from 'react-native';

import { useCompleteOnboarding, useProfile, useSetPersonPhoto } from '@/api/hooks';
import { Icon } from '@/components/Icon';
import { useToast } from '@/components/Toast';
import { Btn, IconBtn, Row, Screen, T } from '@/components/ui';
import { pickPhoto } from '@/domain/photos';
import { fonts, useTheme } from '@/theme';

/**
 * One-time setup on first launch: the full-body photo used for every "See it on me" preview.
 * Opened again from the profile button to replace or remove the photo (`?edit=1`).
 */
export default function Setup() {
  const t = useTheme();
  const toast = useToast();
  const { edit } = useLocalSearchParams<{ edit?: string }>();
  const editing = edit === '1';
  const { data: profile } = useProfile();
  const setPhoto = useSetPersonPhoto();
  const complete = useCompleteOnboarding();
  const [draft, setDraft] = useState<string>();

  const photo = draft ?? profile?.personPhotoUri;

  const choose = async (source: 'camera' | 'library') => {
    const uri = await pickPhoto(source);
    if (uri) setDraft(uri);
  };

  const finish = async (skip = false) => {
    try {
      if (!skip && draft) await setPhoto.mutateAsync(draft);
      if (!profile?.onboarded) await complete.mutateAsync();
      if (editing) {
        toast(draft ? 'Photo updated' : 'No changes');
        router.back();
      } else {
        router.replace('/');
      }
    } catch {
      toast("Couldn't save your photo. Try again.");
    }
  };

  const remove = async () => {
    await setPhoto.mutateAsync(null);
    setDraft(undefined);
    toast('Photo removed');
  };

  return (
    <Screen contentStyle={{ paddingTop: editing ? 14 : 40, flexGrow: 1 }}>
      {editing ? (
        <View style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' }}>
          <IconBtn icon="x" label="Close" onPress={() => router.back()} />
          <T variant="strong">Your try-on photo</T>
          <View style={{ width: 40 }} />
        </View>
      ) : (
        <View style={{ gap: 12 }}>
          <T style={{ fontFamily: fonts.displayItalic, fontSize: 26 }}>Closet</T>
          <T variant="hero" style={{ fontSize: 38, lineHeight: 40 }}>See outfits on you before you get dressed.</T>
          <T tone="muted">Add one full-body photo. Closet uses it to show how each outfit looks on you. You only do this once.</T>
        </View>
      )}

      <View style={{ flexDirection: 'row', gap: 14, alignItems: 'stretch' }}>
        <Pressable
          onPress={() => choose('camera')}
          accessibilityRole="button"
          accessibilityLabel={photo ? 'Retake photo' : 'Take photo'}
          style={{ flex: 1.2, aspectRatio: 3 / 4.4, backgroundColor: t.tile, borderRadius: 3, overflow: 'hidden', alignItems: 'center', justifyContent: 'center' }}
        >
          {photo ? (
            <Image source={{ uri: photo }} style={{ width: '100%', height: '100%' }} resizeMode="cover" />
          ) : (
            <View style={{ alignItems: 'center', gap: 10, padding: 16 }}>
              <Icon name="person" color={t.ink2} size={44} />
              <T variant="small" tone="muted" style={{ textAlign: 'center' }}>Tap to take your photo</T>
            </View>
          )}
        </Pressable>
        <View style={{ flex: 1, gap: 12, justifyContent: 'center' }}>
          <T variant="label">For the best result</T>
          {['Head to shoes in frame', 'Stand straight, arms relaxed', 'Plain background, good light', 'Fitted clothes'].map(tip => (
            <View key={tip} style={{ flexDirection: 'row', gap: 8, alignItems: 'flex-start' }}>
              <Icon name="check" color={t.accent} size={16} />
              <T variant="small" style={{ flex: 1 }}>{tip}</T>
            </View>
          ))}
        </View>
      </View>

      <Row>
        <Btn label={photo ? 'Retake' : 'Take photo'} icon="camera" variant={photo ? 'ghost' : 'primary'} onPress={() => choose('camera')} />
        <Btn label="Choose photo" icon="photos" variant="ghost" onPress={() => choose('library')} />
      </Row>

      <T variant="small" tone="muted">
        Your photo is private. It’s used only to make your previews, and you can replace or delete it any time from your profile.
      </T>

      <View style={{ flex: 1 }} />

      {editing ? (
        <View style={{ gap: 10 }}>
          <Btn label="Save" loading={setPhoto.isPending} onPress={() => finish()} />
          {profile?.personPhotoUri && <Btn label="Delete my photo" variant="ghost" onPress={remove} />}
        </View>
      ) : (
        <View style={{ gap: 10 }}>
          <Btn label="Continue" variant="accent" loading={setPhoto.isPending || complete.isPending} disabled={!draft} onPress={() => finish()}
            style={{ opacity: draft ? 1 : 0.4 }} />
          <Btn label="Do this later" variant="ghost" onPress={() => finish(true)} />
        </View>
      )}
    </Screen>
  );
}
