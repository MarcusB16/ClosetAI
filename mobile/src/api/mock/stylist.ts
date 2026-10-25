import { daysSince } from '@/domain/dates';
import type { Item, OutfitPieces, Slot, StyleContext, StyleRequest } from '@/domain/types';

/**
 * Stand-in for the outfit LLM call. Simple rules so the app behaves believably offline:
 * reads dress level from the request, locks a named item, skips recent wears, respects rain.
 */

export interface StyledResult {
  title: string;
  note: string;
  pieces: OutfitPieces;
  target: number;
}

function mulberry(a: number) {
  return () => {
    a |= 0; a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

const lc = (i: Item) => i.name.charAt(0).toLowerCase() + i.name.slice(1);

export function findMentionedItem(text: string, items: Item[]): Item | undefined {
  const t = text.toLowerCase();
  return items.find(i => t.includes(i.name.toLowerCase()) || (i.keywords ?? []).some(k => t.includes(k)));
}

export function style(req: StyleRequest, items: Item[], ctx: StyleContext, seed: number): StyledResult {
  const t = req.prompt.toLowerCase();
  const rnd = mulberry(seed * 7919);

  let target = 2;
  let mood = 'today';
  if (/formal|wedding|black tie|interview|suit|gala/.test(t)) { target = 3; mood = 'formal'; }
  else if (/business|work|office|meeting|client|smart/.test(t)) { target = 2.4; mood = 'business'; }
  else if (/casual|weekend|relaxed|errand|coffee|brunch|walk/.test(t)) { target = 1.1; mood = 'casual'; }
  if (/dinner|date|drinks/.test(t)) {
    target = mood === 'casual' ? 1.5 : Math.min(target, 1.9);
    mood = mood === 'casual' ? 'dinner-casual' : 'dinner';
  }

  const lock = req.lockItemId ? items.find(i => i.id === req.lockItemId) : findMentionedItem(t, items);
  if (lock && mood === 'today') target = lock.formality;
  const avoid = !!req.avoidRecent || /different|recent|haven.?t|fresh|change it up|rotation/.test(t);

  const ok = (i: Item) =>
    !(avoid && i.id !== lock?.id && i.type !== 'watch' && daysSince(i.lastWorn) <= 7) &&
    !(ctx.rain && i.noRain) &&
    !(i.category === 'formal' && target < 2.8);

  const pick = (slot: Exclude<Slot, 'acc'>): Item => {
    if (lock && lock.slot === slot) return lock;
    const ofSlot = items.filter(i => i.slot === slot);
    let c = ofSlot.filter(i => ok(i) && Math.abs(i.formality - target) <= 1);
    if (!c.length) c = ofSlot.filter(ok);
    if (!c.length) c = ofSlot;
    const w = c.map(i =>
      (1 / (0.3 + Math.abs(i.formality - target))) *
      (target >= 2.8 && i.category === 'formal' ? 5 : 1) *
      (slot === 'outer' && ctx.rain && i.rainOk ? 1.6 : 1));
    let r = rnd() * w.reduce((a, b) => a + b, 0);
    for (let k = 0; k < c.length; k++) { r -= w[k]; if (r <= 0) return c[k]; }
    return c[c.length - 1];
  };

  const outer = pick('outer');
  const pieces: OutfitPieces = { outer: outer.id, top: pick('top').id, bottom: pick('bottom').id, shoes: pick('shoes').id, acc: [] };

  const watch = items.find(i => i.type === 'watch');
  if (watch) pieces.acc.push(watch.id);
  if (lock && lock.slot === 'acc' && lock.id !== watch?.id) pieces.acc.push(lock.id);
  else {
    const types = target >= 2.8 ? ['tie'] : target >= 2.2 ? ['belt', 'scarf'] : target >= 1.6 ? ['belt', 'scarf', 'tote'] : ['beanie', 'tote', 'scarf', 'belt'];
    const pool = items.filter(i => i.slot === 'acc' && types.includes(i.type) && ok(i));
    if (pool.length) pieces.acc.push(pool[Math.floor(rnd() * pool.length)].id);
  }

  const short = lock ? lc(lock) : '';
  const title = lock
    ? `Around the ${short.replace(/^(white|black|brown|dark|light) /, '')}`
    : ({ formal: 'Formal', business: 'Business casual', 'dinner-casual': 'Casual dinner', dinner: 'Dinner', casual: 'Easy casual', today: avoid ? 'Something new' : `${ctx.weekday}${ctx.rain ? ', rain later' : ''}` } as Record<string, string>)[mood];
  const lead = lock
    ? `Built around your ${short}.`
    : avoid ? 'Nothing here was worn in the last week.'
    : ({ formal: 'Sharp and simple.', business: 'Polished enough for a meeting, easy enough for the rest of the day.', 'dinner-casual': 'Relaxed, one step up from daytime.', dinner: 'A little sharper for the evening.', casual: 'Easy pieces that still go together.', today: '' } as Record<string, string>)[mood];
  const out = ctx.rain && outer.rainOk
    ? `${ctx.forecast}, so the ${lc(outer)} goes on top.`
    : `It's ${ctx.tempF}° today, so bring the ${lc(outer)}.`;

  return { title, note: [lead, out].filter(Boolean).join(' '), pieces, target };
}

export function outfitFormality(pieces: OutfitPieces, items: Item[]): number {
  const ids = [pieces.outer, pieces.top, pieces.bottom, pieces.shoes].filter(Boolean) as string[];
  const f = ids.map(id => items.find(i => i.id === id)?.formality ?? 2);
  return f.reduce((a, b) => a + b, 0) / f.length;
}

export { slotFromInstruction } from '@/domain/slots';
