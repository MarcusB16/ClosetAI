import { memo, useMemo } from 'react';
import { Image, StyleSheet, View, type ViewStyle } from 'react-native';
import { SvgXml } from 'react-native-svg';

import type { Item } from '@/domain/types';
import { garmentSvg } from './garmentSvg';

type Props = { item: Pick<Item, 'type' | 'color' | 'variant' | 'imageUri' | 'name'>; style?: ViewStyle };

/** Shows the item's cutout photo, or a drawing when it has none. */
export const Garment = memo(function Garment({ item, style }: Props) {
  const xml = useMemo(() => (item.imageUri ? '' : garmentSvg(item)), [item]);
  return (
    <View style={[styles.box, style]} accessible accessibilityLabel={item.name}>
      {item.imageUri ? (
        <Image source={{ uri: item.imageUri }} style={styles.fill} resizeMode="contain" />
      ) : (
        <SvgXml xml={xml} width="100%" height="100%" />
      )}
    </View>
  );
});

const styles = StyleSheet.create({
  box: { flex: 1, width: '100%' },
  fill: { width: '100%', height: '100%' },
});
