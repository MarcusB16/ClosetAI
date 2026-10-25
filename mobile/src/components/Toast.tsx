import { createContext, useCallback, useContext, useRef, useState } from 'react';
import { Animated, StyleSheet, Text } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { fonts, useTheme } from '@/theme';

const ToastContext = createContext<(msg: string) => void>(() => {});
export const useToast = () => useContext(ToastContext);

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const t = useTheme();
  const insets = useSafeAreaInsets();
  const [msg, setMsg] = useState('');
  const [anim] = useState(() => new Animated.Value(0));
  const timer = useRef<ReturnType<typeof setTimeout>>(undefined);

  const show = useCallback((m: string) => {
    setMsg(m);
    clearTimeout(timer.current);
    Animated.timing(anim, { toValue: 1, duration: 200, useNativeDriver: true }).start();
    timer.current = setTimeout(() => Animated.timing(anim, { toValue: 0, duration: 200, useNativeDriver: true }).start(), 2200);
  }, [anim]);

  return (
    <ToastContext.Provider value={show}>
      {children}
      <Animated.View
        pointerEvents="none"
        accessibilityLiveRegion="polite"
        style={[styles.toast, {
          backgroundColor: t.ink, bottom: insets.bottom + 84, opacity: anim,
          transform: [{ translateY: anim.interpolate({ inputRange: [0, 1], outputRange: [10, 0] }) }],
        }]}
      >
        <Text style={{ color: t.paper, fontFamily: fonts.body, fontSize: 14 }}>{msg}</Text>
      </Animated.View>
    </ToastContext.Provider>
  );
}

const styles = StyleSheet.create({
  toast: { position: 'absolute', left: 18, right: 18, paddingVertical: 12, paddingHorizontal: 16, borderRadius: 12 },
});
