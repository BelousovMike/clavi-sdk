import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { createOidcOptions, ClaviOidcConfiguration, resolveAdminBaseUrl } from './options.js';
import { rolesFromClaims, createPkcePair } from './protocol.js';

describe('createOidcOptions', () => {
  it('builds from partial', () => {
    const o = createOidcOptions({
      authority: 'https://clavi.example/',
      clientId: 'app',
    });
    assert.equal(o.authority, 'https://clavi.example');
    assert.equal(o.callbackPath, '/signin-oidc');
    assert.deepEqual(o.scopes, ['openid', 'profile', 'email']);
  });

  it('fluent ReadFrom.object', () => {
    const o = new ClaviOidcConfiguration()
      .readFrom.object({
        Authority: 'https://clavi.example',
        ClientId: 'app',
        BaseUrl: 'https://clavi.example/',
      })
      .acceptAnyServerCertificate(true)
      .build();
    assert.equal(o.clientId, 'app');
    assert.equal(o.acceptAnyServerCertificate, true);
    assert.equal(resolveAdminBaseUrl(o), 'https://clavi.example/');
  });
});

describe('rolesFromClaims', () => {
  it('merges userinfo and token roles', () => {
    // header.payload.sig — payload {"roles":["a"]}
    const payload = Buffer.from(JSON.stringify({ roles: ['a', 'b'] })).toString('base64url');
    const token = `x.${payload}.y`;
    const roles = rolesFromClaims({
      userInfo: { roles: ['b', 'c'] },
      accessToken: token,
      mapRoles: true,
    });
    assert.deepEqual(roles.sort(), ['a', 'b', 'c']);
  });
});

describe('pkce', () => {
  it('creates verifier/challenge', () => {
    const p = createPkcePair();
    assert.ok(p.verifier.length > 20);
    assert.ok(p.challenge.length > 20);
  });
});
