import { createApi } from '../createApi';
import { HttpClosetApi } from '../http/HttpClosetApi';
import { MockClosetApi } from '../mock/MockClosetApi';

describe('createApi', () => {
  it('returns the HTTP client when a base url is configured', () => {
    expect(createApi('http://10.0.2.2:5000')).toBeInstanceOf(HttpClosetApi);
  });

  it.each([undefined, '', '   '])('keeps the mock when the base url is %p', baseUrl => {
    expect(createApi(baseUrl)).toBeInstanceOf(MockClosetApi);
  });
});
