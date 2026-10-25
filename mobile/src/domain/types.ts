export type Slot = 'outer' | 'top' | 'bottom' | 'shoes' | 'acc';
export type Category = 'shirts' | 'pants' | 'jackets' | 'shoes' | 'accessories' | 'formal';

/** Which drawing to use when an item has no photo. */
export type GarmentType =
  | 'tee' | 'shirt' | 'sweater' | 'blazer' | 'coat' | 'jacket'
  | 'pants' | 'jeans' | 'shorts'
  | 'sneaker' | 'derby' | 'loafer' | 'boot'
  | 'watch' | 'belt' | 'sunglasses' | 'scarf' | 'beanie' | 'tie' | 'tote';

export interface Item {
  id: string;
  name: string;
  brand: string;
  category: Category;
  slot: Slot;
  /** Cutout photo. When missing, the app draws the garment from `type` and `color`. */
  imageUri?: string;
  type: GarmentType;
  /** Drawing variant details: stripe color, denim/bomber/waxed, etc. */
  variant?: Record<string, string>;
  color: string;
  colorName: string;
  /** 1 = casual, 2 = smart, 3 = formal */
  formality: number;
  fabric: string;
  size: string;
  price?: number;
  timesWorn: number;
  lastWorn?: string; // ISO date
  keywords?: string[];
  rainOk?: boolean;
  noRain?: boolean;
  seasons?: string[];
  addedAt: string;
}

export type NewItem = Omit<Item, 'id' | 'timesWorn' | 'lastWorn' | 'addedAt'>;

export interface OutfitPieces {
  outer?: string;
  top: string;
  bottom: string;
  shoes: string;
  acc: string[];
}

export interface Outfit {
  id: string;
  title: string;
  prompt?: string;
  note?: string;
  pieces: OutfitPieces;
  saved: boolean;
  createdAt: string;
}

export interface WearEntry {
  date: string; // ISO date
  outfitId: string;
}

export interface StyleContext {
  date: string;
  weekday: string;
  tempF: number;
  forecast: string;
  rain: boolean;
}

export interface StyleRequest {
  prompt: string;
  lockItemId?: string;
  avoidRecent?: boolean;
}

/** What the tagging call returns for a new photo. The user confirms it before saving. */
export interface ItemSuggestion {
  name: string;
  brand?: string;
  category: Category;
  colorName: string;
  color: string;
  formality: number;
  seasons: string[];
  fabric?: string;
  cutoutUri: string;
  slot?: Slot;
  type?: GarmentType;
}

/** Set once during first-launch setup; used for every try-on. */
export interface Profile {
  onboarded: boolean;
  personPhotoUri?: string;
}

export interface TryOnResult {
  status: 'ready' | 'needs-photo' | 'failed';
  imageUri?: string;
  /** True while the mock stands in for the image model. */
  placeholder?: boolean;
  message?: string;
}

export const SLOT_LABEL: Record<Slot, string> = {
  outer: 'Outerwear', top: 'Top', bottom: 'Bottom', shoes: 'Shoes', acc: 'Accessories',
};

export const CATEGORY_LABEL: Record<Category, string> = {
  shirts: 'Shirts', pants: 'Pants', jackets: 'Jackets', shoes: 'Shoes', accessories: 'Accessories', formal: 'Formalwear',
};
