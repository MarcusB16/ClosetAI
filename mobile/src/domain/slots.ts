import type { Slot } from './types';

/** Which outfit slot a free-text change refers to, e.g. "different shoes" → shoes. */
export function slotFromInstruction(text: string): Slot | undefined {
  const map: [Slot, RegExp][] = [
    ['shoes', /shoe|sneaker|boot|loafer|derb|footwear/],
    ['top', /shirt|top|tee|sweater|knit/],
    ['bottom', /pant|trouser|jean|chino|bottom/],
    ['outer', /jacket|coat|blazer|outer|layer/],
    ['acc', /watch|belt|scarf|accessor|tie|bag|tote/],
  ];
  return map.find(([, r]) => r.test(text.toLowerCase()))?.[0];
}
