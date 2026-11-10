import { createOptions, claviFetch, pagedPath } from '@clavi/sdk-core';
import { Users } from './resources/users.js';
import { Groups } from './resources/groups.js';
import { Roles } from './resources/roles.js';
import { Clients } from './resources/clients.js';
import { Audit } from './resources/audit.js';
import { Tenants } from './resources/tenants.js';
import { Security } from './resources/security.js';
import { Me } from './resources/me.js';

/**
 * Admin API client. Create once, `auth`, then call `users` / `groups` / …
 *
 * @example
 * const client = new ClaviAdminClient({ baseUrl: 'https://clavi.example/' });
 * await client.auth({ username: 'admin', password });
 * const page = await client.users.find({ page: 1, pageSize: 20 });
 */
export class ClaviAdminClient {
  /** @param {Partial<import('@clavi/sdk-core').ClaviClientOptions>} [opts] */
  constructor(opts = {}) {
    this.options = createOptions(opts);
    this.users = new Users(this);
    this.groups = new Groups(this);
    this.roles = new Roles(this);
    this.clients = new Clients(this);
    this.audit = new Audit(this);
    this.tenants = new Tenants(this);
    this.security = new Security(this);
    this.me = new Me(this);
  }

  /** Merge options; keeps the cookie jar unless you pass a new `cookies` map. */
  setConfig(cfg) {
    this.options = createOptions({ ...this.options, ...cfg, cookies: this.options.cookies });
  }

  /** BFF login — session cookie is stored on this client. */
  async auth({ username, password }) {
    await this.#post('api/v1/auth/login', { username, password });
  }

  async logout() {
    await this.#post('api/v1/auth/logout');
  }

  /** Escape hatch for paths that don’t have a typed helper yet. */
  raw(method, path, body) {
    return claviFetch(this.options, path.replace(/^\//, ''), {
      method,
      headers: body === undefined ? undefined : { 'content-type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  }

  /** @internal */
  get(path, query) {
    const p = query ? pagedPath(path, query) : path.replace(/^\//, '');
    return claviFetch(this.options, p);
  }

  /** @internal */
  #post(path, body) {
    return claviFetch(this.options, path.replace(/^\//, ''), {
      method: 'POST',
      headers: body === undefined ? undefined : { 'content-type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  }

  /** @internal */
  post(path, body) {
    return this.#post(path, body);
  }

  /** @internal */
  del(path) {
    return claviFetch(this.options, path.replace(/^\//, ''), { method: 'DELETE' });
  }
}

export default ClaviAdminClient;
