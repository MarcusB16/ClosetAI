import { resolveMediaUrl, stripTrailingSlashes } from '../mediaUrl';

const BASE = 'http://10.0.2.2:5000';

describe('resolveMediaUrl', () => {
  it.each([
    ['/media/a.jpg', `${BASE}/media/a.jpg`],
    ['media/a.jpg', `${BASE}/media/a.jpg`],
    ['http://cdn.test/a.jpg', 'http://cdn.test/a.jpg'],
    ['https://cdn.test/a.jpg', 'https://cdn.test/a.jpg'],
    ['//cdn.test/a.jpg', '//cdn.test/a.jpg'],
    ['data:image/png;base64,AAAA', 'data:image/png;base64,AAAA'],
    ['file:///tmp/a.jpg', 'file:///tmp/a.jpg'],
  ])('resolves %s', (input, expected) => {
    expect(resolveMediaUrl(BASE, input)).toBe(expected);
  });

  it('ignores a trailing slash on the base', () => {
    expect(resolveMediaUrl(`${BASE}/`, '/media/a.jpg')).toBe(`${BASE}/media/a.jpg`);
  });

  it.each([undefined, null, ''])('returns undefined for %p', value => {
    expect(resolveMediaUrl(BASE, value)).toBeUndefined();
  });
});

describe('stripTrailingSlashes', () => {
  it('trims whitespace and slashes', () => {
    expect(stripTrailingSlashes('  http://x:1///  ')).toBe('http://x:1');
  });
});
