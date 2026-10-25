import type {
  Item, ItemSuggestion, NewItem, Outfit, Profile, Slot, StyleContext, StyleRequest, TryOnResult, WearEntry,
} from '@/domain/types';

/**
 * Everything the app needs from the backend. The app only talks to this interface,
 * so the mock can be swapped for an HTTP client against the ASP.NET Core API.
 */
export interface ClosetApi {
  getContext(): Promise<StyleContext>;

  getProfile(): Promise<Profile>;
  /** Upload (or remove, with null) the full-body photo used for try-on. */
  setPersonPhoto(photoUri: string | null): Promise<Profile>;
  completeOnboarding(): Promise<Profile>;

  listItems(): Promise<Item[]>;
  addItem(item: NewItem): Promise<Item>;
  /** Tagging call: background removal + AI suggestions for a new photo. */
  analyzePhoto(photoUri: string): Promise<ItemSuggestion>;

  todaysOutfit(): Promise<Outfit>;
  getOutfit(id: string): Promise<Outfit>;
  /** Outfit call: build an outfit from the closet for a request. */
  styleOutfit(request: StyleRequest): Promise<Outfit>;
  /** Free-text change to an existing outfit, e.g. "different shoes". */
  refineOutfit(outfitId: string, instruction: string): Promise<Outfit>;
  alternatives(outfitId: string, slot: Slot): Promise<Item[]>;
  replacePiece(outfitId: string, slot: Slot, itemId: string): Promise<Outfit>;
  rejectOutfit(outfitId: string): Promise<Outfit>;

  setSaved(outfitId: string, saved: boolean): Promise<Outfit>;
  listSaved(): Promise<Outfit[]>;
  logWear(outfitId: string): Promise<WearEntry>;
  listWorn(): Promise<(WearEntry & { outfit: Outfit })[]>;

  /** Image call: render the outfit on the user's saved full-body photo. */
  tryOn(outfitId: string): Promise<TryOnResult>;

  resetDemoData(): Promise<void>;
}
