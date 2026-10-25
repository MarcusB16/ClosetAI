import type { Item } from '@/domain/types';

/** Builds an SVG drawing of a garment. Placeholder art until items have real cutout photos. */

function shade(hex: string, amt: number): string {
  const n = parseInt(hex.slice(1), 16);
  const f = (v: number) => Math.max(0, Math.min(255, Math.round(amt < 0 ? v * (1 + amt) : v + (255 - v) * amt)));
  return '#' + [n >> 16, (n >> 8) & 255, n & 255].map(v => f(v).toString(16).padStart(2, '0')).join('');
}
function lum(hex: string): number {
  const n = parseInt(hex.slice(1), 16);
  return (0.299 * (n >> 16) + 0.587 * ((n >> 8) & 255) + 0.114 * (n & 255)) / 255;
}

let gid = 0;
type Drawable = Pick<Item, 'type' | 'color' | 'variant'>;

export function garmentSvg(it: Drawable): string {
  const c = it.color, L = lum(c), v = it.variant ?? {};
  const d = L > 0.55 ? shade(c, -0.2) : shade(c, 0.24);
  const o = L > 0.55 ? shade(c, -0.28) : shade(c, -0.35);
  const sw = `stroke="${o}" stroke-width=".7" stroke-linejoin="round"`;
  const ln = (p: string, w = 0.8, col = d, x = '') =>
    `<path d="${p}" fill="none" stroke="${col}" stroke-width="${w}" stroke-linecap="round" stroke-linejoin="round" ${x}/>`;
  const dash = 'stroke-dasharray="1.3 1"';
  let s = '';

  switch (it.type) {
    case 'tee': {
      const B = 'M33 14 L42 10 Q50 17 58 10 L67 14 L84 28 L77 39 L68 33 L68 90 L32 90 L32 33 L23 39 L16 28 Z';
      s = `<path d="${B}" fill="${c}"/>`;
      if (v.stripe) {
        const id = 'cp' + ++gid;
        let l = '';
        for (let y = 24; y <= 88; y += 6.5) l += `<path d="M10 ${y} H90" stroke="${v.stripe}" stroke-width="2.4"/>`;
        s += `<defs><clipPath id="${id}"><path d="${B}"/></clipPath></defs><g clip-path="url(#${id})">${l}</g>`;
      }
      s += `<path d="${B}" fill="none" ${sw}/>` + ln('M42 10 Q50 17 58 10', 1.5) + ln('M32 86 H68', 0.5) + ln('M75.5 37 L82.5 30', 0.5) + ln('M24.5 37 L17.5 30', 0.5);
      break;
    }
    case 'shirt': {
      const B = 'M34 14 L44 10 L50 15 L56 10 L66 14 L74 22 L81 72 L73 74 L67 38 L67 92 L33 92 L33 38 L27 74 L19 72 L26 22 Z';
      s = `<path d="${B}" fill="${c}" ${sw}/>`;
      s += `<path d="M44 10 L50 15 L47 22 L41.5 14 Z" fill="${c}" ${sw}/><path d="M56 10 L50 15 L53 22 L58.5 14 Z" fill="${c}" ${sw}/>`;
      s += ln('M50 16 V92', 0.6) + [30, 43, 56, 69, 82].map(y => `<circle cx="51.6" cy="${y}" r=".9" fill="${d}"/>`).join('');
      s += ln('M27.5 68.5 L20 66.8', 0.6) + ln('M72.5 68.5 L80 66.8', 0.6);
      if (v.pocket) s += ln('M55 30 H62 V37 Q58.5 38.5 55 37 Z', 0.55);
      break;
    }
    case 'sweater': {
      const B = 'M34 14 L43 11 Q50 17 57 11 L66 14 L74 22 L80 72 L73 74 L67 38 L67 90 L33 90 L33 38 L27 74 L20 72 L26 22 Z';
      s = `<path d="${B}" fill="${c}" ${sw}/>` + ln('M43 11 Q50 17 57 11', 1.8) + ln('M44.5 13.8 Q50 19 55.5 13.8', 0.45);
      s += ln('M33 84.5 H67', 0.6) + ln('M27.3 67.5 L20.6 66', 0.6) + ln('M72.7 67.5 L79.4 66', 0.6);
      if (v.cable) {
        const w = (x: number) => `M${x} 22 ` + Array.from({ length: 8 }, (_, k) => `Q${x + (k % 2 ? -3 : 3)} ${26 + k * 8} ${x} ${30 + k * 8}`).join(' ');
        s += ln(w(42), 0.9) + ln(w(58), 0.9) + ln('M50 20 V84', 0.5) + ln('M46 20 V84', 0.35) + ln('M54 20 V84', 0.35);
      }
      break;
    }
    case 'blazer':
    case 'coat': {
      const coat = it.type === 'coat', hem = coat ? 96 : 88, vv = coat ? 40 : 46;
      const B = `M33 13 L44 9 L50 12 L56 9 L67 13 L75 21 L81 74 L73 76 L67 38 L67 ${hem} Q59 ${hem + 2} 51 ${hem} L49 ${hem} Q41 ${hem + 2} 33 ${hem} L33 38 L27 76 L19 74 L25 21 Z`;
      s = `<path d="${B}" fill="${c}" ${sw}/>`;
      s += `<path d="M44 9 L50 12 L56 9 L50 ${vv} Z" fill="${v.inner ?? '#EDEBE5'}"/>`;
      s += `<path d="M44 9 L39 23 L44 25.5 L41.5 28 L50 ${vv} L58.5 28 L56 25.5 L61 23 L56 9" fill="${c}" ${sw}/>`;
      s += ln(`M44 9 L41.5 28 L50 ${vv}`, 0.6) + ln(`M56 9 L58.5 28 L50 ${vv}`, 0.6);
      s += ln(`M50 ${vv} V${hem}`, 0.6) + `<circle cx="51.5" cy="${vv + 6}" r="1.1" fill="${d}"/><circle cx="51.5" cy="${vv + 15}" r="1.1" fill="${d}"/>`;
      s += ln(`M36 ${hem - 22} H45`, 0.8) + ln(`M55 ${hem - 22} H64`, 0.8) + ln('M56 33 H63', 0.7);
      s += ln('M27.4 71 L19.5 69.2', 0.5) + ln('M72.6 71 L80.5 69.2', 0.5);
      break;
    }
    case 'jacket': {
      const B = 'M33 14 L44 10 L50 13 L56 10 L67 14 L75 22 L80 70 L73 72 L67 38 L67 80 L33 80 L33 38 L27 72 L20 70 L25 22 Z';
      s = `<path d="${B}" fill="${c}" ${sw}/>`;
      if (v.style === 'denim') {
        const st = '#C98A3A';
        s += `<path d="M44 10 L50 13 L46.5 21 L41 14 Z" fill="${c}" ${sw}/><path d="M56 10 L50 13 L53.5 21 L59 14 Z" fill="${c}" ${sw}/>`;
        s += ln('M33 29 Q50 33 67 29', 0.6, st, dash) + ln('M37 36 H46 V44 H37 Z', 0.6, st, dash) + ln('M54 36 H63 V44 H54 Z', 0.6, st, dash) + ln('M50 14 V80', 0.6, st, dash) + ln('M33 74 H67', 0.6, st, dash) + ln('M48 25 V74', 0.5, st, dash) + ln('M52 25 V74', 0.5, st, dash);
        s += [24, 36, 50, 64].map(y => `<circle cx="51" cy="${y}" r="1" fill="#B9A27A"/>`).join('');
      } else if (v.style === 'bomber') {
        const rib = shade(c, 0.1);
        s += `<path d="M33 74 H67 V80 H33 Z" fill="${rib}"/>` + ln('M44 10 Q50 15.5 56 10', 2.2, rib) + ln('M50 14 V80', 0.8, '#8E8E8E') + ln('M69.5 34 H75 V40 H70', 0.5) + ln('M27.6 68 L20.4 66.3', 2, rib) + ln('M72.4 68 L79.6 66.3', 2, rib);
      } else {
        s += `<path d="M44 10 L50 13 L46 20 L40.5 14 Z" fill="#5A3A22"/><path d="M56 10 L50 13 L54 20 L59.5 14 Z" fill="#5A3A22"/>`;
        s += `<path d="M35.5 52 H47 V58 H35.5 Z" fill="${shade(c, 0.06)}" ${sw}/><path d="M53 52 H64.5 V58 H53 Z" fill="${shade(c, 0.06)}" ${sw}/>` + ln('M35.5 58 V70 H47 V58', 0.5) + ln('M53 58 V70 H64.5 V58', 0.5);
        s += ln('M50 13 V80', 0.7) + [26, 38, 50, 62, 74].map(y => `<circle cx="52" cy="${y}" r="1" fill="#A89A73"/>`).join('');
      }
      break;
    }
    case 'pants':
    case 'jeans': {
      s = `<path d="M30 8 H70 L74 96 H55 L50 32 L45 96 H26 Z" fill="${c}" ${sw}/>` + ln('M30 14 H70', 0.7) + ln('M50 14 V30', 0.6) + ln('M50 30 Q53.5 29 53.5 24 V14', 0.5) + ln('M31 16 Q38 17 40 25', 0.6) + ln('M69 16 Q62 17 60 25', 0.6);
      s += [35, 45, 55, 65].map(x => ln(`M${x} 7.5 V15`, 1.3, o)).join('');
      if (v.crease) s += ln('M37.5 30 L36 95', 0.35) + ln('M62.5 30 L64 95', 0.35);
      if (it.type === 'jeans') {
        const st = '#C98A3A';
        s += ln('M31 16 Q38 17 40 25', 0.6, st, dash) + ln('M69 16 Q62 17 60 25', 0.6, st, dash) + ln('M50.8 14 V30', 0.5, st, dash) + ln('M26.4 91 H44.6', 0.5, st, dash) + ln('M55.4 91 H73.6', 0.5, st, dash) + `<circle cx="50" cy="11" r="1.2" fill="#B9A27A"/>`;
      }
      break;
    }
    case 'shorts':
      s = `<path d="M29 22 H71 L76 70 H54 L50 46 L46 70 H24 Z" fill="${c}" ${sw}/>` + ln('M29 28 H71', 0.7) + ln('M50 28 V42', 0.6) + ln('M30 30 Q37 31 39 38', 0.6) + ln('M70 30 Q63 31 61 38', 0.6) + ln('M24.5 65 H46.4', 0.5) + ln('M53.6 65 H75.5', 0.5);
      break;
    case 'sneaker': {
      const soleBase = v.sole ?? '#F4F4F0';
      const one = (tx: number, ty: number, back: boolean) => {
        const sole = back ? shade(soleBase, -0.08) : soleBase, cc = back ? shade(c, -0.1) : c;
        let g = `<g transform="translate(${tx} ${ty})"><path d="M14 66 Q12 52 22 49 L38 46 Q46 38 56 35 L66 34 Q70 44 82 51 Q91 56 89 66 Z" fill="${cc}" ${sw}/>`;
        g += `<path d="M11 65 H88 Q92 65 91 70 Q90 75 85 75 H15 Q10 75 10 70 Q10 65 11 65 Z" fill="${sole}" stroke="${shade(sole, -0.2)}" stroke-width=".6"/>`;
        if (!back) {
          g += ln('M44 44.5 L50 48.5', 0.9) + ln('M48 41.5 L54 45.5', 0.9) + ln('M52 38.5 L58 42.5', 0.9) + ln('M15 62 Q14 54 20 51', 0.5) + ln('M11 70 H90', 0.4, shade(sole, -0.18));
          if (v.stripe) g += ln('M30 61 Q52 60 74 51', 2.2, v.stripe);
        }
        return g + '</g>';
      };
      s = one(6, -4, true) + one(-2, 12, false);
      break;
    }
    case 'derby':
    case 'loafer': {
      const one = (tx: number, ty: number, back: boolean) => {
        const cc = back ? shade(c, -0.12) : c, so = L < 0.2 ? '#3A3A3A' : shade(c, -0.55);
        let g = `<g transform="translate(${tx} ${ty})"><path d="M12 68 Q10 58 20 55 L44 51 Q54 44 64 44 Q78 46 87 58 Q91 64 88 68 Z" fill="${cc}" ${sw}/><path d="M9 67 H89 Q93 67 91 71 H11 Q7 71 9 67 Z" fill="${so}"/><path d="M10 71 H24 V74 H11 Z" fill="${so}"/>`;
        if (!back) {
          if (it.type === 'loafer') g += ln('M44 51.5 Q57 61 70 47.5', 0.8) + ln('M51 53.5 L64 50.5', 1.8, shade(c, -0.25)) + ln('M74 47 Q86 53 88 66', 0.4, shade(c, 0.35));
          else {
            g += ln('M45 51.5 Q52 59 61 48.5', 0.7) + ln('M50 49.5 L56 53.5', 0.9) + ln('M53.5 47.5 L59.5 51.5', 0.9);
            if (v.captoe) g += ln('M75 48 Q71 58 80 67', 0.6);
            if (L < 0.2) g += ln('M72 47 Q84 51 87 60', 0.5, '#5A5A5A');
          }
        }
        return g + '</g>';
      };
      s = one(6, -6, true) + one(-2, 12, false);
      break;
    }
    case 'boot': {
      const one = (tx: number, ty: number, back: boolean) => {
        const cc = back ? shade(c, -0.1) : c, el = shade(c, L < 0.3 ? 0.14 : -0.15), so = L < 0.2 ? '#3C3C3C' : shade(c, -0.5);
        let g = `<g transform="translate(${tx} ${ty})"><path d="M12 72 Q10 62 22 59 L40 56 L41 16 H63 L64 50 Q82 53 88 64 Q91 70 88 72 Z" fill="${cc}" ${sw}/>`;
        g += `<path d="M45.5 20 H58 L59 50 Q52 54 45 52 Z" fill="${el}"/><path d="M60 17 L61.5 8.5 L66 10 L64 17 Z" fill="${shade(c, 0.2)}"/>`;
        g += `<path d="M9 71 H89 Q93 71 91 76 H11 Q7 76 9 71 Z" fill="${so}"/>`;
        if (!back) g += ln('M68 52 Q83 55 87 64', 0.5, '#5A5A5A');
        return g + '</g>';
      };
      s = one(8, -6, true) + one(-4, 8, false);
      break;
    }
    case 'watch': {
      const strap = v.strap ?? '#463A2C', dial = v.dial ?? '#1D2331', st = shade(strap, 0.3);
      s = `<rect x="43" y="4" width="14" height="92" rx="4" fill="${strap}"/>` + ln('M45 6 V36', 0.4, st, dash) + ln('M55 6 V36', 0.4, st, dash) + ln('M45 64 V94', 0.4, st, dash) + ln('M55 64 V94', 0.4, st, dash);
      [74, 80, 86].forEach(y => (s += `<circle cx="50" cy="${y}" r="1" fill="${shade(strap, -0.4)}"/>`));
      s += `<circle cx="50" cy="50" r="18" fill="${c}" stroke="${shade(c, -0.3)}" stroke-width=".8"/><circle cx="50" cy="50" r="14.5" fill="${dial}"/><rect x="67" y="47.5" width="4" height="5" rx="1" fill="${c}"/>`;
      for (let k = 0; k < 12; k++) {
        const a = (k * Math.PI) / 6, r1 = k % 3 ? 12.5 : 11.5;
        s += `<line x1="${50 + Math.sin(a) * r1}" y1="${50 - Math.cos(a) * r1}" x2="${50 + Math.sin(a) * 13.6}" y2="${50 - Math.cos(a) * 13.6}" stroke="#F2EFE6" stroke-width="${k % 3 ? 0.5 : 0.9}"/>`;
      }
      s += ln('M50 50 L50 40', 1.2, '#F2EFE6') + ln('M50 50 L57 54', 1.2, '#F2EFE6');
      break;
    }
    case 'belt':
      s = `<path d="M8 66 Q50 30 92 66" fill="none" stroke="${o}" stroke-width="11" stroke-linecap="round"/><path d="M8 66 Q50 30 92 66" fill="none" stroke="${c}" stroke-width="9.6" stroke-linecap="round"/>` + ln('M8 66 Q50 30 92 66', 0.4, d, dash);
      [[72, 52], [77, 55.5], [82, 59.5]].forEach(([x, y]) => (s += `<circle cx="${x}" cy="${y}" r="1.1" fill="${shade(c, -0.5)}"/>`));
      s += `<rect x="14" y="48" width="13" height="18" rx="2" fill="none" stroke="#B8B6AE" stroke-width="2.4" transform="rotate(-38 20.5 57)"/>`;
      break;
    case 'sunglasses':
      s = `<path d="M13 42 H46 Q47 60 38 64 H22 Q13 62 13 42 Z" fill="#2A2118" stroke="${c}" stroke-width="3"/><path d="M54 42 H87 Q87 62 78 64 H62 Q53 60 54 42 Z" fill="#2A2118" stroke="${c}" stroke-width="3"/>`;
      s += ln('M46 45 Q50 41 54 45', 2.6, c) + ln('M11 42 L6 40', 2.6, c) + ln('M89 42 L94 40', 2.6, c) + ln('M18 47 L24 47', 1, '#6B655E') + ln('M59 47 L65 47', 1, '#6B655E');
      break;
    case 'scarf':
      s = `<path d="M28 6 H52 V82 H28 Z" fill="${c}" ${sw}/><path d="M46 24 H70 V90 H46 Z" fill="${shade(c, -0.08)}" ${sw}/>`;
      s += ln('M28 14 H52', 1.4, shade(c, 0.25)) + ln('M46 32 H70', 1.4, shade(c, 0.18));
      for (let x = 29; x <= 51; x += 2.5) s += ln(`M${x} 82 V88`, 0.7, c);
      for (let x = 47; x <= 69; x += 2.5) s += ln(`M${x} 90 V96`, 0.7, shade(c, -0.08));
      break;
    case 'beanie':
      s = `<path d="M22 72 Q20 26 50 24 Q80 26 78 72 Z" fill="${c}" ${sw}/><path d="M19 60 H81 V77 Q50 81 19 77 Z" fill="${shade(c, 0.06)}" ${sw}/>`;
      for (let x = 24; x <= 76; x += 4) s += ln(`M${x} 61 V77.5`, 0.5);
      for (let x = 30; x <= 70; x += 5) s += ln(`M${x} 32 Q${x} 46 ${x} 59`, 0.35);
      break;
    case 'tie':
      s = `<path d="M46 6 H54 L56 15 L52 20 L60 82 L50 94 L40 82 L48 20 L44 15 Z" fill="${c}" ${sw}/>` + ln('M44 15 H56', 0.6) + ln('M48 20 H52', 0.6);
      for (let y = 30; y <= 84; y += 7) s += ln(`M43 ${y} L57 ${y - 5}`, 0.35, shade(c, 0.2));
      break;
    case 'tote': {
      const trim = v.trim ?? '#2C3A5C';
      s = ln('M34 40 Q34 12 50 12 Q66 12 66 40', 3.4, trim) + `<path d="M20 38 H80 L76 92 H24 Z" fill="${c}" ${sw}/><path d="M20.6 44 H79.4 L79 50 H21.1 Z" fill="${trim}"/>` + ln('M24 86 H76', 0.4, d, dash);
      break;
    }
  }
  return `<svg viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">${s}</svg>`;
}
