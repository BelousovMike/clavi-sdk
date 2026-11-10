import { createHash, randomBytes } from 'node:crypto';

/** @param {number} [nbytes] */
export function createPkcePair(nbytes = 32) {
  const verifier = base64Url(randomBytes(nbytes));
  const challenge = base64Url(createHash('sha256').update(verifier).digest());
  return { verifier, challenge, method: 'S256' };
}

function base64Url(buf) {
  return Buffer.from(buf).toString('base64').replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

/**
 * @param {string} authority
 * @param {{ fetch?: typeof fetch, rejectUnauthorized?: boolean }} [http]
 */
export async function discovery(authority, http = {}) {
  const base = authority.replace(/\/$/, '');
  const url = `${base}/.well-known/openid-configuration`;
  const res = await (http.fetch ?? fetch)(url);
  if (!res.ok) throw new Error(`OIDC discovery failed: HTTP ${res.status}`);
  return res.json();
}

/**
 * @param {object} p
 * @param {string} p.authorizationEndpoint
 * @param {string} p.clientId
 * @param {string} p.redirectUri
 * @param {string} p.scope
 * @param {string} p.state
 * @param {string} p.nonce
 * @param {string} p.codeChallenge
 * @param {string} [p.codeChallengeMethod]
 */
export function buildAuthorizeUrl(p) {
  const u = new URL(p.authorizationEndpoint);
  u.searchParams.set('client_id', p.clientId);
  u.searchParams.set('response_type', 'code');
  u.searchParams.set('redirect_uri', p.redirectUri);
  u.searchParams.set('scope', p.scope);
  u.searchParams.set('state', p.state);
  u.searchParams.set('nonce', p.nonce);
  u.searchParams.set('code_challenge', p.codeChallenge);
  u.searchParams.set('code_challenge_method', p.codeChallengeMethod ?? 'S256');
  return u.toString();
}

/**
 * @param {object} p
 * @param {string} p.tokenEndpoint
 * @param {string} p.clientId
 * @param {string} [p.clientSecret]
 * @param {string} p.code
 * @param {string} p.redirectUri
 * @param {string} p.codeVerifier
 * @param {{ fetch?: typeof fetch }} [http]
 */
export async function exchangeCode(p, http = {}) {
  const body = new URLSearchParams({
    grant_type: 'authorization_code',
    client_id: p.clientId,
    code: p.code,
    redirect_uri: p.redirectUri,
    code_verifier: p.codeVerifier,
  });
  if (p.clientSecret) body.set('client_secret', p.clientSecret);

  const res = await (http.fetch ?? fetch)(p.tokenEndpoint, {
    method: 'POST',
    headers: { 'content-type': 'application/x-www-form-urlencoded' },
    body,
  });
  if (!res.ok) {
    const t = await res.text();
    throw new Error(`Token exchange failed: HTTP ${res.status}: ${t.slice(0, 200)}`);
  }
  return res.json();
}

/**
 * @param {string} userinfoEndpoint
 * @param {string} accessToken
 * @param {{ fetch?: typeof fetch }} [http]
 */
export async function fetchUserInfo(userinfoEndpoint, accessToken, http = {}) {
  const res = await (http.fetch ?? fetch)(userinfoEndpoint, {
    headers: { authorization: `Bearer ${accessToken}` },
  });
  if (!res.ok) throw new Error(`userinfo failed: HTTP ${res.status}`);
  return res.json();
}

export function endSessionUrl(endSessionEndpoint, { idToken, postLogoutRedirectUri }) {
  if (!endSessionEndpoint) return null;
  const u = new URL(endSessionEndpoint);
  if (idToken) u.searchParams.set('id_token_hint', idToken);
  if (postLogoutRedirectUri) u.searchParams.set('post_logout_redirect_uri', postLogoutRedirectUri);
  return u.toString();
}

export function decodeJwtPayload(jwt) {
  if (!jwt || typeof jwt !== 'string') return null;
  const parts = jwt.split('.');
  if (parts.length < 2) return null;
  try {
    const json = Buffer.from(parts[1].replace(/-/g, '+').replace(/_/g, '/'), 'base64').toString('utf8');
    return JSON.parse(json);
  } catch {
    return null;
  }
}

/** Collect unique role strings from userinfo and/or access token. */
export function rolesFromClaims({ userInfo, accessToken, mapRoles = true }) {
  if (!mapRoles) return [];
  const roles = new Set();
  const add = (arr) => {
    if (!Array.isArray(arr)) return;
    for (const r of arr) {
      if (typeof r === 'string' && r.trim()) roles.add(r.trim());
    }
  };
  add(userInfo?.roles);
  const payload = decodeJwtPayload(accessToken);
  add(payload?.roles);
  return [...roles];
}
