import assert from 'node:assert/strict';
import test from 'node:test';
import { pagedPath } from './index.js';

test('pagedPath builds query', () => {
  assert.equal(pagedPath('api/v1/users', { page: 2, pageSize: 10, search: ' a ' }), 'api/v1/users?page=2&page_size=10&search=a');
});
