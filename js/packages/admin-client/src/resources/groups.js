/** @param {import('../index.js').ClaviAdminClient} client */
export class Groups {
  constructor(client) {
    this.client = client;
  }

  /** @param {{ page?: number, pageSize?: number, search?: string }} [q] */
  find(q) {
    return this.client.get('api/v1/groups', q ?? {});
  }

  findOne({ id }) {
    return this.client.get(`api/v1/groups/${id}`);
  }

  create(payload) {
    return this.client.post('api/v1/groups', payload);
  }

  del({ id }) {
    return this.client.del(`api/v1/groups/${id}`);
  }
}
