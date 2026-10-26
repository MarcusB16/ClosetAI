import type { Item, ItemSuggestion, Outfit, Profile, TryOnResult, WearEntry } from '@/domain/types';
import { resolveMediaUrl } from './mediaUrl';

export interface WireTryOnResult {
  status: string;
  imageUri?: string;
  placeholder?: boolean;
  message?: string;
}

export type WireWornOutfit = WearEntry & { outfit: Outfit };

const DATE_ONLY_LENGTH = 10;

const toDateOnly = (value: string | undefined) => value?.slice(0, DATE_ONLY_LENGTH);

export function toItem(baseUrl: string, wire: Item): Item {
  return { ...wire, imageUri: resolveMediaUrl(baseUrl, wire.imageUri), lastWorn: toDateOnly(wire.lastWorn) };
}

export function toProfile(baseUrl: string, wire: Profile): Profile {
  return { ...wire, personPhotoUri: resolveMediaUrl(baseUrl, wire.personPhotoUri) };
}

export function toSuggestion(baseUrl: string, wire: ItemSuggestion): ItemSuggestion {
  return { ...wire, cutoutUri: resolveMediaUrl(baseUrl, wire.cutoutUri) ?? wire.cutoutUri };
}

export function toWearEntry(wire: WearEntry): WearEntry {
  return { date: toDateOnly(wire.date) ?? wire.date, outfitId: wire.outfitId };
}

export function toWornOutfit(wire: WireWornOutfit): WireWornOutfit {
  return { ...toWearEntry(wire), outfit: wire.outfit };
}

export function toTryOnResult(baseUrl: string, wire: WireTryOnResult): TryOnResult {
  return {
    status: toTryOnStatus(wire.status),
    imageUri: resolveMediaUrl(baseUrl, wire.imageUri),
    placeholder: wire.placeholder,
    message: wire.message,
  };
}

function toTryOnStatus(status: string): TryOnResult['status'] {
  if (status === 'ready') return 'ready';
  if (status === 'needsPhoto') return 'needs-photo';
  return 'failed';
}
