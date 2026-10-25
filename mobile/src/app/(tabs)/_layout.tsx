import { Redirect, router, Tabs } from 'expo-router';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { useProfile } from '@/api/hooks';
import { Icon, type IconName } from '@/components/Icon';
import { ErrorScreen } from '@/components/ui';
import { fonts, useTheme } from '@/theme';

type TabBarProps = Parameters<NonNullable<React.ComponentProps<typeof Tabs>['tabBar']>>[0];

const TABS: { name: string; label: string; icon: IconName }[] = [
  { name: 'index', label: 'Today', icon: 'home' },
  { name: 'closet', label: 'Closet', icon: 'hanger' },
  { name: 'add', label: 'Add', icon: 'plus' },
  { name: 'style', label: 'Style', icon: 'spark' },
  { name: 'outfits', label: 'Outfits', icon: 'stack' },
];

function TabBar({ state, navigation }: TabBarProps) {
  const t = useTheme();
  const insets = useSafeAreaInsets();
  const current = state.routes[state.index]?.name;
  return (
    <View style={[styles.bar, { backgroundColor: t.paper, borderColor: t.line, paddingBottom: Math.max(insets.bottom, 10) }]}>
      {TABS.map(tab => {
        if (tab.name === 'add') {
          return (
            <Pressable key="add" onPress={() => router.push('/add')} accessibilityRole="button" accessibilityLabel="Add a piece" style={styles.item}>
              <View style={[styles.plus, { backgroundColor: t.ink }]}><Icon name="plus" color={t.paper} /></View>
            </Pressable>
          );
        }
        const active = current === tab.name;
        const color = active ? t.ink : t.ink2;
        return (
          <Pressable key={tab.name} onPress={() => navigation.navigate(tab.name)} accessibilityRole="tab"
            accessibilityState={{ selected: active }} accessibilityLabel={tab.label} style={styles.item}>
            <Icon name={tab.icon} color={color} size={22} />
            <Text style={[styles.label, { color }]}>{tab.label}</Text>
          </Pressable>
        );
      })}
    </View>
  );
}

export default function TabsLayout() {
  const { data: profile, isError, refetch } = useProfile();
  if (isError && !profile) return <ErrorScreen message="Couldn't reach Closet." onRetry={() => refetch()} />;
  if (profile && !profile.onboarded) return <Redirect href="/setup" />;
  return (
    <Tabs screenOptions={{ headerShown: false }} tabBar={props => <TabBar {...props} />}>
      <Tabs.Screen name="index" />
      <Tabs.Screen name="closet" />
      <Tabs.Screen name="style" />
      <Tabs.Screen name="outfits" />
    </Tabs>
  );
}

const styles = StyleSheet.create({
  bar: { flexDirection: 'row', borderTopWidth: StyleSheet.hairlineWidth, paddingTop: 8, paddingHorizontal: 6 },
  item: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 3 },
  label: { fontFamily: fonts.body, fontSize: 10.5 },
  plus: { width: 44, height: 44, borderRadius: 22, alignItems: 'center', justifyContent: 'center' },
});
