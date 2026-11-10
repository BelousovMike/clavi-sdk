import test from 'node:test';
import assert from 'node:assert/strict';
import { ClaviAdminClient } from './index.js';

test('resources exist after construct', () => {
  process.env.CLAVI_BASE_URL = 'https://example.test/';
  const c = new ClaviAdminClient();
  assert.equal(typeof c.auth, 'function');
  assert.equal(typeof c.users.find, 'function');
  assert.equal(typeof c.groups.find, 'function');
  assert.equal(typeof c.me.get, 'function');
});
