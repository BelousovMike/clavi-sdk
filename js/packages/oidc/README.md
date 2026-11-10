# `@clavi/oidc`

Shared Clavi OIDC options, PKCE, discovery, and role mapping.

```js
import { ClaviOidcConfiguration, createOidcOptions, createPkcePair } from '@clavi/oidc';

const opts = new ClaviOidcConfiguration()
  .readFrom.object({ Authority: 'https://clavi.example', ClientId: 'my-app' })
  .acceptAnyServerCertificate(true)
  .build();
```

See [`docs/oidc-adapters.md`](../../../docs/oidc-adapters.md).
