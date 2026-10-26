#!/usr/bin/env node
/*
 * Throwaway stand-in for the ClosetAI backend, following docs/api-contract.md.
 * Lets the mobile app run against the HTTP client before the real API exists.
 *
 *   npm run stub-server            # http://localhost:5099
 *   PORT=5100 STUB_LATENCY_MS=1500 npm run stub-server
 *   STUB_FAIL=style,tryOn npm run stub-server   # make those routes return 502 problem+json
 *
 * Data is in memory and resets when the process restarts (or on POST /api/demo/reset).
 */
const http = require('node:http');
const { randomUUID } = require('node:crypto');

const PORT = Number(process.env.PORT || 5099);
const LATENCY_MS = Number(process.env.STUB_LATENCY_MS || 300);
const FAIL = new Set((process.env.STUB_FAIL || '').split(',').map(s => s.trim()).filter(Boolean));

const day = offset => new Date(Date.now() - offset * 86400000).toISOString().slice(0, 10);
const nowIso = () => new Date().toISOString();

const piece = (id, slot, category, type, name, brand, color, colorName, formality, extra = {}) => ({
  id, slot, category, type, name, brand, color, colorName, formality, fabric: 'Cotton', size: 'M', timesWorn: 5,
  addedAt: nowIso(), lastWorn: day(20), ...extra,
});

const seedItems = () => [
  piece('t1', 'top', 'shirts', 'shirt', 'White oxford shirt', 'J.Crew', '#F3F2EE', 'White', 2, { lastWorn: day(200) }),
  piece('t2', 'top', 'shirts', 'tee', 'Black crew tee', 'COS', '#1F1F21', 'Black', 1),
  piece('t3', 'top', 'shirts', 'sweater', 'Navy merino crewneck', 'Everlane', '#25314F', 'Navy', 2, { lastWorn: day(150) }),
  piece('b1', 'bottom', 'pants', 'jeans', 'Dark indigo jeans', 'A.P.C.', '#24334F', 'Indigo', 1),
  piece('b2', 'bottom', 'pants', 'pants', 'Stone chinos', 'Bonobos', '#C8B898', 'Stone', 2),
  piece('s1', 'shoes', 'shoes', 'sneaker', 'White leather sneakers', 'Common Projects', '#F2F1EC', 'White', 1),
  piece('s2', 'shoes', 'shoes', 'loafer', 'Brown penny loafers', 'G.H. Bass', '#5B3A22', 'Dark brown', 2),
  piece('s3', 'shoes', 'shoes', 'boot', 'Black chelsea boots', 'R.M. Williams', '#1B1B1C', 'Black', 2),
  piece('o1', 'outer', 'jackets', 'blazer', 'Navy blazer', 'Suitsupply', '#1F2842', 'Navy', 2),
  piece('o2', 'outer', 'jackets', 'jacket', 'Denim jacket', "Levi's", '#5D7DA6', 'Mid wash', 1, { variant: { style: 'denim' } }),
  piece('a1', 'acc', 'accessories', 'watch', 'Steel field watch', 'Hamilton', '#C9CBCF', 'Steel', 2),
  piece('a2', 'acc', 'accessories', 'belt', 'Brown leather belt', "Anderson's", '#5A3A22', 'Brown', 2),
];

let state;
const media = new Map();
let lookCounter;

function reset() {
  lookCounter = 0;
  state = {
    items: seedItems(),
    profile: { onboarded: false },
    outfits: new Map(),
    wears: [],
    todayId: null,
  };
  state.todayId = createOutfit('Easy Sunday', { top: 't2', bottom: 'b1', shoes: 's1', outer: 'o2', acc: ['a1'] }).id;
  createOutfit('Client lunch', { top: 't1', bottom: 'b2', shoes: 's2', outer: 'o1', acc: ['a1', 'a2'] }, { saved: true });
}

function createOutfit(title, pieces, extra = {}) {
  const outfit = { id: `o${++lookCounter}`, title, pieces, saved: false, createdAt: nowIso(), ...extra };
  state.outfits.set(outfit.id, outfit);
  return outfit;
}

const bySlot = slot => state.items.filter(i => i.slot === slot);
const pick = (slot, n) => { const list = bySlot(slot); return list[n % list.length]; };

function buildOutfit(prompt, lockItemId) {
  const n = lookCounter + 1;
  const pieces = {
    top: pick('top', n).id, bottom: pick('bottom', n).id, shoes: pick('shoes', n).id, outer: pick('outer', n).id, acc: [pick('acc', n).id],
  };
  const locked = state.items.find(i => i.id === lockItemId);
  if (locked) {
    if (locked.slot === 'acc') pieces.acc = [locked.id];
    else pieces[locked.slot] = locked.id;
  }
  return createOutfit(prompt.length > 28 ? `${prompt.slice(0, 26)}…` : prompt, pieces, {
    prompt,
    note: `Built from your closet for “${prompt}”. (stub server)`,
  });
}

class Problem extends Error {
  constructor(status, title, detail, errors) {
    super(detail || title);
    Object.assign(this, { status, title, detail, errors });
  }
}

const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));

function readBody(req) {
  return new Promise(resolve => {
    const chunks = [];
    req.on('data', c => chunks.push(c));
    req.on('end', () => resolve(Buffer.concat(chunks)));
  });
}

function json(req, body) {
  if (!body.length) return {};
  try { return JSON.parse(body.toString('utf8')); } catch { throw new Problem(400, 'Bad Request', 'Body is not valid JSON.'); }
}

function readPhoto(req, body) {
  const type = req.headers['content-type'] || '';
  const boundary = /boundary=(?:"([^"]+)"|([^;]+))/.exec(type);
  if (!type.startsWith('multipart/form-data') || !boundary) throw new Problem(400, 'Bad Request', 'Expected multipart/form-data.');
  const raw = body.toString('latin1');
  const parts = raw.split(`--${boundary[1] || boundary[2]}`);
  const part = parts.find(p => /name="photo"/.test(p));
  if (!part) throw new Problem(400, 'Validation failed', 'The photo field is required.', { photo: ['The photo field is required.'] });
  const headerEnd = part.indexOf('\r\n\r\n');
  const mime = /Content-Type:\s*([^\r\n]+)/i.exec(part.slice(0, headerEnd))?.[1]?.trim() || 'image/jpeg';
  const bytes = Buffer.from(part.slice(headerEnd + 4, part.lastIndexOf('\r\n')), 'latin1');
  if (!['image/jpeg', 'image/png'].includes(mime)) throw new Problem(400, 'Validation failed', 'Only JPEG or PNG photos are accepted.');
  if (bytes.length > 10 * 1024 * 1024) throw new Problem(413, 'Payload Too Large', 'Photos can be at most 10 MB.');
  const fileName = `${randomUUID()}.${mime === 'image/png' ? 'png' : 'jpg'}`;
  media.set(fileName, { mime, bytes });
  return fileName;
}

const profileView = () => state.profile;
const outfitOr404 = id => state.outfits.get(id) || (() => { throw new Problem(404, 'Not Found', `Outfit ${id} was not found.`); })();

function maybeFail(name) {
  if (FAIL.has(name)) throw new Problem(502, 'Bad Gateway', `The AI provider failed (${name} forced by STUB_FAIL).`);
}

const routes = [
  ['GET', /^\/api\/context$/, () => ({ date: day(0), weekday: new Date().toLocaleDateString('en-US', { weekday: 'long' }), tempF: 58, forecast: 'Clear, light breeze', rain: false })],
  ['GET', /^\/api\/profile$/, () => profileView()],
  ['PUT', /^\/api\/profile\/person-photo$/, ({ req, body }) => {
    state.profile = { ...state.profile, personPhotoUri: `/media/${readPhoto(req, body)}` };
    return profileView();
  }],
  ['DELETE', /^\/api\/profile\/person-photo$/, () => { delete state.profile.personPhotoUri; return profileView(); }],
  ['PATCH', /^\/api\/profile$/, ({ req, body }) => {
    const { onboarded } = json(req, body);
    if (typeof onboarded === 'boolean') state.profile.onboarded = onboarded;
    return profileView();
  }],
  ['GET', /^\/api\/items$/, () => state.items],
  ['POST', /^\/api\/items$/, ({ req, body }) => {
    const input = json(req, body);
    const errors = {};
    if (!input.name) errors.name = ['Name is required.'];
    if (!Number.isInteger(input.formality)) errors.formality = ['Formality must be an integer (1-3).'];
    if (Object.keys(errors).length) throw new Problem(400, 'One or more validation errors occurred.', undefined, errors);
    const item = { ...input, id: `n${randomUUID().slice(0, 8)}`, timesWorn: 0, addedAt: nowIso() };
    state.items.push(item);
    return { status: 201, headers: { Location: `/api/items/${item.id}` }, body: item };
  }],
  ['POST', /^\/api\/items\/analyze$/, ({ req, body }) => {
    maybeFail('analyze');
    const file = readPhoto(req, body);
    return {
      name: 'Grey crew sweatshirt', brand: 'Uniqlo', category: 'shirts', colorName: 'Grey', color: '#9A9CA0', formality: 1,
      seasons: ['Fall', 'Winter'], fabric: 'Cotton fleece', cutoutUri: `/media/${file}`, slot: 'top', type: 'sweater',
    };
  }],
  ['GET', /^\/api\/outfits\/today$/, () => outfitOr404(state.todayId)],
  ['GET', /^\/api\/outfits\/saved$/, () => [...state.outfits.values()].filter(o => o.saved)],
  ['POST', /^\/api\/outfits\/style$/, ({ req, body }) => {
    maybeFail('style');
    const { prompt, lockItemId } = json(req, body);
    if (!prompt) throw new Problem(400, 'One or more validation errors occurred.', undefined, { prompt: ['Prompt is required.'] });
    return { status: 201, body: buildOutfit(prompt, lockItemId) };
  }],
  ['GET', /^\/api\/outfits\/([^/]+)\/alternatives$/, ({ match, url }) => {
    const o = outfitOr404(decodeURIComponent(match[1]));
    const slot = url.searchParams.get('slot');
    const current = slot === 'acc' ? o.pieces.acc : [o.pieces[slot]];
    return bySlot(slot).filter(i => !current.includes(i.id));
  }],
  ['PUT', /^\/api\/outfits\/([^/]+)\/pieces\/([^/]+)$/, ({ req, body, match }) => {
    const o = outfitOr404(decodeURIComponent(match[1]));
    const slot = decodeURIComponent(match[2]);
    const item = state.items.find(i => i.id === json(req, body).itemId);
    if (!item || item.slot !== slot) throw new Problem(400, 'Bad Request', `That item does not belong to the ${slot} slot.`);
    if (slot === 'acc') o.pieces.acc = [item.id, ...o.pieces.acc.slice(1)];
    else o.pieces[slot] = item.id;
    return o;
  }],
  ['PUT', /^\/api\/outfits\/([^/]+)\/saved$/, ({ req, body, match }) => {
    const o = outfitOr404(decodeURIComponent(match[1]));
    o.saved = !!json(req, body).saved;
    return o;
  }],
  ['POST', /^\/api\/outfits\/([^/]+)\/refine$/, ({ req, body, match }) => {
    maybeFail('refine');
    const o = outfitOr404(decodeURIComponent(match[1]));
    const instruction = json(req, body).instruction || '';
    const slot = /shoe/i.test(instruction) ? 'shoes' : /top|shirt/i.test(instruction) ? 'top' : /pant|jean|bottom/i.test(instruction) ? 'bottom' : null;
    if (slot) {
      const list = bySlot(slot);
      o.pieces[slot] = list[(list.findIndex(i => i.id === o.pieces[slot]) + 1) % list.length].id;
    }
    o.note = `Changed per “${instruction}”. (stub server)`;
    return o;
  }],
  ['POST', /^\/api\/outfits\/([^/]+)\/reject$/, ({ match }) => {
    outfitOr404(decodeURIComponent(match[1]));
    return buildOutfit('Something different', undefined);
  }],
  ['POST', /^\/api\/outfits\/([^/]+)\/wears$/, ({ match }) => {
    const o = outfitOr404(decodeURIComponent(match[1]));
    const entry = { date: day(0), outfitId: o.id };
    state.wears.unshift(entry);
    [o.pieces.outer, o.pieces.top, o.pieces.bottom, o.pieces.shoes, ...o.pieces.acc].forEach(id => {
      const item = state.items.find(i => i.id === id);
      if (item) { item.timesWorn += 1; item.lastWorn = entry.date; }
    });
    return { status: 201, body: entry };
  }],
  ['GET', /^\/api\/wears$/, () => state.wears.map(w => ({ ...w, outfit: state.outfits.get(w.outfitId) }))],
  ['GET', /^\/api\/outfits\/([^/]+)$/, ({ match }) => outfitOr404(decodeURIComponent(match[1]))],
  ['POST', /^\/api\/outfits\/([^/]+)\/try-on$/, ({ match }) => {
    outfitOr404(decodeURIComponent(match[1]));
    maybeFail('tryOn');
    if (!state.profile.personPhotoUri) return { status: 'needsPhoto' };
    return { status: 'ready', imageUri: state.profile.personPhotoUri, placeholder: true };
  }],
  ['POST', /^\/api\/demo\/reset$/, () => { reset(); return { status: 204 }; }],
];

const CORS = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Methods': 'GET,POST,PUT,PATCH,DELETE,OPTIONS',
  'Access-Control-Allow-Headers': '*',
  'Access-Control-Expose-Headers': 'Location',
};

function send(res, status, body, headers = {}) {
  const isProblem = status >= 400;
  const payload = body === undefined ? '' : JSON.stringify(body);
  res.writeHead(status, {
    ...CORS,
    ...(payload ? { 'Content-Type': isProblem ? 'application/problem+json' : 'application/json' } : {}),
    ...headers,
  });
  res.end(payload);
}

const server = http.createServer(async (req, res) => {
  const url = new URL(req.url, `http://${req.headers.host}`);
  const started = Date.now();
  const log = status => console.log(`${req.method} ${url.pathname}${url.search} -> ${status} (${Date.now() - started}ms)`);

  if (req.method === 'OPTIONS') { res.writeHead(204, CORS); res.end(); return; }

  const mediaMatch = /^\/media\/([^/]+)$/.exec(url.pathname);
  if (req.method === 'GET' && mediaMatch) {
    const file = media.get(decodeURIComponent(mediaMatch[1]));
    if (!file) { send(res, 404, { title: 'Not Found', status: 404 }); log(404); return; }
    res.writeHead(200, { ...CORS, 'Content-Type': file.mime });
    res.end(file.bytes);
    log(200);
    return;
  }

  try {
    const body = await readBody(req);
    await sleep(LATENCY_MS);
    const route = routes.find(([method, pattern]) => method === req.method && pattern.test(url.pathname));
    if (!route) throw new Problem(404, 'Not Found', `No route for ${req.method} ${url.pathname}.`);
    const result = route[2]({ req, body, url, match: route[1].exec(url.pathname) });
    if (result && result.status && typeof result.status === 'number') {
      const { status, body: payload, headers } = result;
      send(res, status, payload, headers);
      log(status);
    } else {
      send(res, 200, result);
      log(200);
    }
  } catch (error) {
    const problem = error instanceof Problem ? error : new Problem(500, 'Internal Server Error', error.message);
    send(res, problem.status, { type: 'about:blank', title: problem.title, status: problem.status, detail: problem.detail, errors: problem.errors });
    log(problem.status);
  }
});

reset();
server.listen(PORT, '0.0.0.0', () => {
  console.log(`ClosetAI stub server listening on http://0.0.0.0:${PORT} (latency ${LATENCY_MS}ms, forced failures: ${[...FAIL].join(', ') || 'none'})`);
});
