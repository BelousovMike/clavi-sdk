/** @param {import('../index.js').ClaviAdminClient} client */
export class Clients {
  constructor(client) {
    this.client = client;
  }

  /** @param {{ page?: number, pageSize?: number, search?: string }} [q] */
  find(q) {
    return this.client.get('api/v1/oidc/clients', q ?? {});
  }

  findOne({ id }) {
    return this.client.get(`api/v1/oidc/clients/${id}`);
  }
}
