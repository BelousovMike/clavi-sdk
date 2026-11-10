import { ClaviAdminClient } from '@clavi/admin-client';
import {
  ClaviOidcConfiguration,
  createOidcOptions,
  resolveAdminBaseUrl,
  rolesFromClaims,
  decodeJwtPayload,
} from '@clavi/oidc';

export { ClaviOidcConfiguration, createOidcOptions };

/**
 * Auth.js / NextAuth OAuth provider configured for Clavi.
 *
 * @param {import('@clavi/oidc').ClaviOidcOptions | ((c: ClaviOidcConfiguration) => void) | Record<string, unknown>} [configure]
 * @returns {Record<string, unknown>} NextAuth provider object
 *
 * @example
 * import NextAuth from 'next-auth';
 * import { ClaviProvider, createClaviAdmin } from '@clavi/next';
 * export const { handlers, auth } = NextAuth({
 *   providers: [ClaviProvider((c) => c.readFrom.env())],
 *   callbacks: { … use mapClaviSession … }
 * });
 */
export function ClaviProvider(configure) {
  const options = resolve(configure);
  const issuer = options.authority.replace(/\/$/, '');
  return {
    id: 'clavi',
    name: 'Clavi',
    type: 'oidc',
    issuer,
    clientId: options.clientId,
    clientSecret: options.clientSecret || undefined,
    wellKnown: `${issuer}/.well-known/openid-configuration`,
    authorization: {
      params: {
        scope: options.scopes.join(' '),
        response_type: 'code',
      },
    },
    checks: ['pkce', 'state'],
    profile(profile) {
      const roles = options.mapRoles && Array.isArray(profile.roles) ? profile.roles : [];
      return {
        id: profile.sub,
        name: profile.preferred_username || profile.name || profile.email,
        email: profile.email,
        image: null,
        roles,
      };
    },
  };
}

/**
 * Merge Clavi roles into the Auth.js session user.
 * Call from `callbacks.jwt` / `callbacks.session`.
 */
export function mapClaviSession({ token, session, profile, account }) {
  const roles = rolesFromClaims({
    userInfo: profile,
    accessToken: account?.access_token || token?.accessToken,
    mapRoles: true,
  });
  if (token) {
    if (roles.length) token.roles = roles;
    if (account?.access_token) token.accessToken = account.access_token;
  }
  if (session?.user) {
    session.user.roles = token?.roles || roles || session.user.roles || [];
  }
  return { token, session };
}

export function createClaviAdmin(configure) {
  const options = resolve(configure);
  return new ClaviAdminClient({
    baseUrl: resolveAdminBaseUrl(options),
    tenantHost: options.tenantHost,
  });
}

/** Decode roles from a JWT access token (edge helpers). */
export function rolesFromAccessToken(accessToken) {
  return rolesFromClaims({ accessToken, mapRoles: true });
}

export { decodeJwtPayload };

function resolve(configure) {
  if (typeof configure === 'function') {
    const c = new ClaviOidcConfiguration();
    configure(c);
    return c.build();
  }
  if (configure?.authority || configure?.clientId) return createOidcOptions(configure);
  if (configure && typeof configure === 'object') {
    return new ClaviOidcConfiguration().readFrom.object(configure).build();
  }
  return createOidcOptions({});
}
