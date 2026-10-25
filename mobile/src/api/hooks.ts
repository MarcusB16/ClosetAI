import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import type { Item, NewItem, Outfit, Slot, StyleRequest } from '@/domain/types';
import { api } from './index';

export const keys = {
  context: ['context'] as const,
  items: ['items'] as const,
  today: ['outfit', 'today'] as const,
  outfit: (id: string) => ['outfit', id] as const,
  saved: ['saved'] as const,
  worn: ['worn'] as const,
  profile: ['profile'] as const,
};

export const useProfile = () => useQuery({ queryKey: keys.profile, queryFn: () => api.getProfile() });

export function useSetPersonPhoto() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (uri: string | null) => api.setPersonPhoto(uri),
    onSuccess: p => {
      qc.setQueryData(keys.profile, p);
      qc.removeQueries({ queryKey: ['tryOn'] }); // old previews show the old photo
    },
  });
}

export function useCompleteOnboarding() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: () => api.completeOnboarding(), onSuccess: p => qc.setQueryData(keys.profile, p) });
}

/** One try-on per outfit look. Ready results are cached, so reopening the same outfit doesn't call the image model again. */
export const useTryOn = (outfit: Outfit | undefined) =>
  useQuery({
    queryKey: ['tryOn', outfit?.id, JSON.stringify(outfit?.pieces)],
    queryFn: () => api.tryOn(outfit!.id),
    enabled: !!outfit,
    staleTime: query => (query.state.data?.status === 'ready' ? Infinity : 0),
    gcTime: 1000 * 60 * 60,
    retry: 0,
  });

export const useContext = () => useQuery({ queryKey: keys.context, queryFn: () => api.getContext() });
export const useItems = () => useQuery({ queryKey: keys.items, queryFn: () => api.listItems() });
export const useTodaysOutfit = () => useQuery({ queryKey: keys.today, queryFn: () => api.todaysOutfit() });
export const useOutfit = (id: string) => useQuery({ queryKey: keys.outfit(id), queryFn: () => api.getOutfit(id), enabled: !!id });
export const useSaved = () => useQuery({ queryKey: keys.saved, queryFn: () => api.listSaved() });
export const useWorn = () => useQuery({ queryKey: keys.worn, queryFn: () => api.listWorn() });

/** Map of item id → item, for resolving outfit pieces. */
export function useItemMap() {
  const { data } = useItems();
  const map = new Map<string, Item>();
  data?.forEach(i => map.set(i.id, i));
  return map;
}

function usePutOutfit() {
  const qc = useQueryClient();
  return (o: Outfit) => {
    qc.setQueryData(keys.outfit(o.id), o);
    qc.invalidateQueries({ queryKey: keys.saved });
    qc.invalidateQueries({ queryKey: keys.today });
    qc.invalidateQueries({ queryKey: ['alternatives', o.id] });
  };
}

export function useStyleOutfit() {
  const put = usePutOutfit();
  return useMutation({ mutationFn: (req: StyleRequest) => api.styleOutfit(req), onSuccess: put });
}

export function useRefineOutfit() {
  const put = usePutOutfit();
  return useMutation({ mutationFn: (v: { id: string; instruction: string }) => api.refineOutfit(v.id, v.instruction), onSuccess: put });
}

export function useReplacePiece() {
  const put = usePutOutfit();
  return useMutation({ mutationFn: (v: { id: string; slot: Slot; itemId: string }) => api.replacePiece(v.id, v.slot, v.itemId), onSuccess: put });
}

export function useRejectOutfit() {
  const put = usePutOutfit();
  return useMutation({ mutationFn: (id: string) => api.rejectOutfit(id), onSuccess: put });
}

export function useSetSaved() {
  const put = usePutOutfit();
  return useMutation({ mutationFn: (v: { id: string; saved: boolean }) => api.setSaved(v.id, v.saved), onSuccess: put });
}

export function useLogWear() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (outfitId: string) => api.logWear(outfitId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.worn });
      qc.invalidateQueries({ queryKey: keys.items });
    },
  });
}

export function useAddItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (item: NewItem) => api.addItem(item),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.items }),
  });
}

export const useAlternatives = (outfitId: string, slot: Slot | null) =>
  useQuery({
    queryKey: ['alternatives', outfitId, slot],
    queryFn: () => api.alternatives(outfitId, slot!),
    enabled: !!slot,
  });
