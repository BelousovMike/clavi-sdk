import { randomBytes } from 'node:crypto';
import { ClaviAdminClient } from '@clavi/admin-client';
import {
  ClaviOidcConfiguration,
  createOidcOptions,
  resolveAdminBaseUrl,
  createPkcePair,
  discovery,
  buildAuthorizeUrl,
  exchangeCode,
  fetchUserInfo,
  endSessionUrl,
  rolesFromClaims,
} from '@clavi/oidc';

export { ClaviOidcConfiguration, createOidcOptions };

/**
 * Attach Clavi OIDC routes + optional Admin client on `app.locals.claviAdmin`.
 *
 * Requires `express-session` (or compatible) so `req.session` exists.
 *
 * @param {import('express').Express} app
 * @param {import('@clavi/oidc').ClaviOidcOptions | ((c: ClaviOidcConfiguration) => void) | Record<string, unknown>} [configure]
 * @param {{ publicOrigin?: string, loginPath?: string, logoutPath?: string, mePath?: string }} [opts]
 */
export function addClaviOpenIdConnect(app, configure, opts = {}) {
  const options = resolveConfigure(configure);
  const loginPath = opts.loginPath ?? '/login';
  const logoutPath = opts.logoutPath ?? '/logout';
  const mePath = opts.mePath ?? '/me';
  const publicOrigin = opts.publicOrigin;

  /** @type {Promise<any>|null} */
  let discoveryPromise = null;
  const getDiscovery = () => {
    discoveryPromise ??= discovery(options.authority);
    return discoveryPromise;
  };

  app.locals.claviOidc = options;
  app.locals.claviAdmin = new ClaviAdminClient({
    baseUrl: resolveAdminBaseUrl(options),
    tenantHost: options.tenantHost,
  });

  app.get(loginPath, async (req, res, next) => {
    try {
      ensureSession(req);
      const meta = await getDiscovery();
      const pkce = createPkcePair();
      const state = base64url(randomBytes(16));
      const nonce = base64url(randomBytes(16));
      req.session.claviOidc = {
        verifier: pkce.verifier,
        state,
        nonce,
        returnTo: typeof req.query.returnTo === 'string' ? req.query.returnTo : '/',
      };
      const redirectUri = absoluteUrl(req, options.callbackPath, publicOrigin);
      const url = buildAuthorizeUrl({
        authorizationEndpoint: meta.authorization_endpoint,
        clientId: options.clientId,
        redirectUri,
        scope: options.scopes.join(' '),
        state,
        nonce,
        codeChallenge: pkce.challenge,
      });
      res.redirect(url);
    } catch (err) {
      next(err);
    }
  });

  app.get(options.callbackPath, async (req, res, next) => {
    try {
      ensureSession(req);
      const pending = req.session.claviOidc;
      if (!pending) {
        res.status(400).send('missing OIDC session');
        return;
      }
      if (req.query.error) {
        res.status(400).send(String(req.query.error_description || req.query.error));
        return;
      }
      if (req.query.state !== pending.state) {
        res.status(400).send('invalid state');
        return;
      }
      const meta = await getDiscovery();
      const redirectUri = absoluteUrl(req, options.callbackPath, publicOrigin);
      const tokens = await exchangeCode({
        tokenEndpoint: meta.token_endpoint,
        clientId: options.clientId,
        clientSecret: options.clientSecret,
        code: String(req.query.code || ''),
        redirectUri,
        codeVerifier: pending.verifier,
      });
      const userInfo = meta.userinfo_endpoint
        ? await fetchUserInfo(meta.userinfo_endpoint, tokens.access_token)
        : {};
      const roles = rolesFromClaims({
        userInfo,
        accessToken: tokens.access_token,
        mapRoles: options.mapRoles,
      });
      req.session.claviUser = {
        sub: userInfo.sub,
        name: userInfo.preferred_username || userInfo.name || userInfo.email,
        email: userInfo.email,
        roles,
        idToken: tokens.id_token,
        accessToken: tokens.access_token,
      };
      delete req.session.claviOidc;
      res.redirect(pending.returnTo || '/');
    } catch (err) {
      next(err);
    }
  });

  app.get(logoutPath, async (req, res, next) => {
    try {
      ensureSession(req);
      const user = req.session.claviUser;
      const meta = await getDiscovery().catch(() => null);
      const postLogout = absoluteUrl(req, options.signedOutCallbackPath, publicOrigin);
      delete req.session.claviUser;
      delete req.session.claviOidc;
      const url = meta?.end_session_endpoint
        ? endSessionUrl(meta.end_session_endpoint, {
          idToken: user?.idToken,
          postLogoutRedirectUri: postLogout,
        })
        : null;
      if (url) res.redirect(url);
      else res.redirect(options.signedOutCallbackPath);
    } catch (err) {
      next(err);
    }
  });

  app.get(options.signedOutCallbackPath, (_req, res) => {
    res.redirect('/');
  });

  app.get(mePath, (req, res) => {
    ensureSession(req);
    if (!req.session.claviUser) {
      res.status(401).json({ authenticated: false });
      return;
    }
    const { idToken, accessToken, ...safe } = req.session.claviUser;
    res.json({ authenticated: true, user: safe });
  });

  return { options, admin: app.locals.claviAdmin };
}

/** Express middleware: require signed-in Clavi user; optional role list. */
export function requireClaviAuth(roles = []) {
  const need = Array.isArray(roles) ? roles : [roles];
  return (req, res, next) => {
    const user = req.session?.claviUser;
    if (!user) {
      res.status(401).json({ error: 'unauthorized' });
      return;
    }
    req.user = user;
    if (need.length && !need.some((r) => user.roles?.includes(r))) {
      res.status(403).json({ error: 'forbidden' });
      return;
    }
    next();
  };
}

/** Create Admin client from the same options surface. */
export function createClaviAdmin(configure) {
  const options = resolveConfigure(configure);
  return new ClaviAdminClient({
    baseUrl: resolveAdminBaseUrl(options),
    tenantHost: options.tenantHost,
  });
}

function resolveConfigure(configure) {
  if (typeof configure === 'function') {
    const c = new ClaviOidcConfiguration();
    configure(c);
    return c.build();
  }
  if (configure && typeof configure === 'object' && (configure.Authority || configure.Clavi || configure.authority)) {
    if (configure.authority || configure.clientId) return createOidcOptions(configure);
    return new ClaviOidcConfiguration().readFrom.object(configure).build();
  }
  return createOidcOptions(configure || {});
}

function ensureSession(req) {
  if (!req.session) {
    throw new Error('@clavi/express requires express-session (or compatible) middleware');
  }
}

function absoluteUrl(req, path, publicOrigin) {
  const origin = publicOrigin
    || `${req.protocol}://${req.get('x-forwarded-host') || req.get('host')}`;
  return new URL(path, origin.endsWith('/') ? origin : `${origin}/`).toString();
}

function base64url(buf) {
  return Buffer.from(buf).toString('base64').replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}
