/** @param {import('../index.js').ClaviAdminClient} client */
export class Tenants {
  constructor(client) {
    this.client = client;
  }

  /** @param {{ page?: number, pageSize?: number }} [q] */
  find(q) {
    return this.client.get('api/v1/system/tenants', q ?? {});
  }

  findOne({ id }) {
    return this.client.get(`api/v1/system/tenants/${id}`);
  }
}
