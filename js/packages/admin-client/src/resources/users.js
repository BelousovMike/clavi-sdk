/** Users under `/api/v1/users`. */
export class Users {
  /** @param {import('../index.js').ClaviAdminClient} client */
  constructor(client) {
    this.client = client;
  }

  /** Paged list; pass `search` to filter. */
  find(q) {
    return this.client.get('api/v1/users', q ?? {});
  }

  findOne({ id }) {
    return this.client.get(`api/v1/users/${id}`);
  }

  create(payload) {
    return this.client.post('api/v1/users', payload);
  }

  del({ id }) {
    return this.client.del(`api/v1/users/${id}`);
  }

  listRoles({ id }) {
    return this.client.get(`api/v1/users/${id}/roles`);
  }
}
