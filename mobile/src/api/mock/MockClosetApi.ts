import AsyncStorage from '@react-native-async-storage/async-storage';

import type { ClosetApi } from '@/api/ClosetApi';
import { daysAgoIso, isoDay } from '@/domain/dates';
import type {
  Item, ItemSuggestion, NewItem, Outfit, OutfitPieces, Profile, Slot, StyleContext, StyleRequest, TryOnResult, WearEntry,
} from '@/domain/types';
import { ootdIds, seedItems, seedOutfits } from './seed';
import { outfitFormality, slotFromInstruction, style } from './stylist';

const KEY = 'closet.mock.v1';

interface Db {
  items: Item[];
  outfits: Outfit[];
  worn: WearEntry[];
  ootd: { date: string; outfitId: string };
  seed: number;
  profile: Profile;
}

const latency = (ms = 250) => new Promise(r => setTimeout(r, ms));
const uid = () => Math.random().toString(36).slice(2, 10);

function piecesFromIds(ids: string[], items: Item[]): OutfitPieces {
  const p: OutfitPieces = { top: '', bottom: '', shoes: '', acc: [] };
  for (const id of ids) {
    const i = items.find(x => x.id === id);
    if (!i) continue;
    if (i.slot === 'acc') p.acc.push(i.id);
    else p[i.slot] = i.id;
  }
  return p;
}

function seedDb(): Db {
  const now = new Date().toISOString();
  const items: Item[] = seedItems.map(({ lastDaysAgo, ...i }) => ({ ...i, lastWorn: daysAgoIso(lastDaysAgo), addedAt: now }));
  const outfits: Outfit[] = seedOutfits.map(o => ({
    id: o.id, title: o.title, pieces: piecesFromIds(o.ids, items), saved: !!o.saved, createdAt: now,
  }));
  const worn: WearEntry[] = seedOutfits.filter(o => o.wornDaysAgo).map(o => ({ date: daysAgoIso(o.wornDaysAgo!), outfitId: o.id }));
  // Keep "last worn" consistent with the example history.
  for (const w of worn) {
    const o = outfits.find(x => x.id === w.outfitId)!;
    allIds(o.pieces).forEach(id => {
      const i = items.find(x => x.id === id);
      if (i && (!i.lastWorn || w.date > i.lastWorn)) i.lastWorn = w.date;
    });
  }
  return { items, outfits, worn, ootd: { date: '', outfitId: '' }, seed: 3, profile: { onboarded: false } };
}

function allIds(p: OutfitPieces): string[] {
  return [p.outer, p.top, p.bottom, p.shoes, ...p.acc].filter(Boolean) as string[];
}

export class MockClosetApi implements ClosetApi {
  private db?: Db;

  private async load(): Promise<Db> {
    if (this.db) return this.db;
    const raw = await AsyncStorage.getItem(KEY);
    this.db = raw ? (JSON.parse(raw) as Db) : seedDb();
    this.db.profile ??= { onboarded: false };
    if (!raw) await this.persist();
    return this.db;
  }

  private async persist() {
    if (this.db) await AsyncStorage.setItem(KEY, JSON.stringify(this.db));
  }

  private outfit(db: Db, id: string): Outfit {
    const o = db.outfits.find(x => x.id === id);
    if (!o) throw new Error(`Outfit ${id} not found`);
    return o;
  }

  async getContext(): Promise<StyleContext> {
    const d = new Date();
    return {
      date: isoDay(d),
      weekday: d.toLocaleDateString('en-US', { weekday: 'long' }),
      tempF: 57,
      forecast: 'Rain after 4pm',
      rain: true,
    };
  }

  async getProfile() {
    const db = await this.load();
    return { ...db.profile };
  }

  async setPersonPhoto(photoUri: string | null) {
    const db = await this.load();
    await latency(600);
    db.profile.personPhotoUri = photoUri ?? undefined;
    await this.persist();
    return { ...db.profile };
  }

  async completeOnboarding() {
    const db = await this.load();
    db.profile.onboarded = true;
    await this.persist();
    return { ...db.profile };
  }

  async listItems() {
    const db = await this.load();
    return [...db.items];
  }

  async addItem(item: NewItem): Promise<Item> {
    const db = await this.load();
    await latency();
    const created: Item = { ...item, id: uid(), timesWorn: 0, addedAt: new Date().toISOString() };
    db.items.unshift(created);
    await this.persist();
    return created;
  }

  async analyzePhoto(photoUri: string): Promise<ItemSuggestion> {
    await latency(1200);
    // The real call returns a background-removed cutout and AI tags. The mock echoes the photo.
    return {
      name: '', category: 'shirts', colorName: '', color: '#8A8A84',
      formality: 1.5, seasons: ['Fall', 'Winter'], cutoutUri: photoUri,
    };
  }

  async todaysOutfit(): Promise<Outfit> {
    const db = await this.load();
    const today = isoDay();
    if (db.ootd.date === today) return this.outfit(db, db.ootd.outfitId);
    const ctx = await this.getContext();
    const o: Outfit = {
      id: uid(), title: `${ctx.weekday}, rain later`, saved: false, createdAt: new Date().toISOString(),
      pieces: piecesFromIds(ootdIds, db.items),
      note: 'Rain is due after 4pm, so the olive field jacket goes on top. Chelsea boots handle the puddles.',
    };
    db.outfits.push(o);
    db.ootd = { date: today, outfitId: o.id };
    await this.persist();
    return o;
  }

  async getOutfit(id: string) {
    return this.outfit(await this.load(), id);
  }

  async styleOutfit(req: StyleRequest): Promise<Outfit> {
    const db = await this.load();
    await latency(1400);
    const r = style(req, db.items, await this.getContext(), ++db.seed);
    const o: Outfit = { id: uid(), title: r.title, note: r.note, prompt: req.prompt, pieces: r.pieces, saved: false, createdAt: new Date().toISOString() };
    db.outfits.push(o);
    await this.persist();
    return o;
  }

  async refineOutfit(outfitId: string, instruction: string): Promise<Outfit> {
    const db = await this.load();
    const o = this.outfit(db, outfitId);
    const slot = slotFromInstruction(instruction);
    if (slot) {
      const alts = await this.alternatives(outfitId, slot);
      if (alts.length) return this.replacePiece(outfitId, slot, alts[Math.floor(Math.random() * alts.length)].id);
      return o;
    }
    return this.styleOutfit({ prompt: [o.prompt, instruction].filter(Boolean).join(' ') });
  }

  async alternatives(outfitId: string, slot: Slot): Promise<Item[]> {
    const db = await this.load();
    const o = this.outfit(db, outfitId);
    const ctx = await this.getContext();
    const target = outfitFormality(o.pieces, db.items);
    const current = slot === 'acc' ? o.pieces.acc[1] : o.pieces[slot];
    return db.items.filter(i =>
      i.slot === slot && i.id !== current && i.type !== 'watch' &&
      !(ctx.rain && i.noRain) && Math.abs(i.formality - target) <= 1.3);
  }

  async replacePiece(outfitId: string, slot: Slot, itemId: string): Promise<Outfit> {
    const db = await this.load();
    const o = this.outfit(db, outfitId);
    if (slot === 'acc') {
      const acc = [...o.pieces.acc];
      acc[1] = itemId; // acc[0] is the everyday watch
      o.pieces = { ...o.pieces, acc };
    } else {
      o.pieces = { ...o.pieces, [slot]: itemId };
    }
    o.saved = false;
    await this.persist();
    return { ...o };
  }

  async rejectOutfit(outfitId: string): Promise<Outfit> {
    const db = await this.load();
    const o = this.outfit(db, outfitId);
    return this.styleOutfit({ prompt: o.prompt ?? 'What should I wear today?' });
  }

  async setSaved(outfitId: string, saved: boolean): Promise<Outfit> {
    const db = await this.load();
    const o = this.outfit(db, outfitId);
    o.saved = saved;
    await this.persist();
    return { ...o };
  }

  async listSaved() {
    const db = await this.load();
    return db.outfits.filter(o => o.saved).reverse();
  }

  async logWear(outfitId: string): Promise<WearEntry> {
    const db = await this.load();
    const o = this.outfit(db, outfitId);
    const date = isoDay();
    const previous = db.worn.find(w => w.date === date);
    if (previous) {
      const prevOutfit = db.outfits.find(x => x.id === previous.outfitId);
      prevOutfit && allIds(prevOutfit.pieces).forEach(id => {
        const i = db.items.find(x => x.id === id);
        if (i) i.timesWorn = Math.max(0, i.timesWorn - 1);
      });
      db.worn = db.worn.filter(w => w.date !== date);
    }
    allIds(o.pieces).forEach(id => {
      const i = db.items.find(x => x.id === id);
      if (i) { i.timesWorn += 1; i.lastWorn = date; }
    });
    const entry = { date, outfitId };
    db.worn.push(entry);
    await this.persist();
    return entry;
  }

  async listWorn() {
    const db = await this.load();
    return [...db.worn]
      .sort((a, b) => b.date.localeCompare(a.date))
      .map(w => ({ ...w, outfit: this.outfit(db, w.outfitId) }));
  }

  async tryOn(outfitId: string): Promise<TryOnResult> {
    const db = await this.load();
    this.outfit(db, outfitId);
    const photo = db.profile.personPhotoUri;
    if (!photo) return { status: 'needs-photo' };
    await latency(3500);
    // No image model in the mock: hand back the person photo so the screen flow can be tried.
    return { status: 'ready', imageUri: photo, placeholder: true };
  }

  async resetDemoData() {
    this.db = seedDb();
    await this.persist();
  }
}
