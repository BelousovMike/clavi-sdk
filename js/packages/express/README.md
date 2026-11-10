# `@clavi/express`

Clavi OIDC login + Admin client for Express (same config surface as `Clavi.Sdk.AspNetCore`).

```js
import express from 'express';
import session from 'express-session';
import { addClaviOpenIdConnect, requireClaviAuth } from '@clavi/express';

const app = express();
app.use(session({ secret: 'dev', resave: false, saveUninitialized: false }));

// Fluent + env (CLAVI_AUTHORITY, CLAVI_CLIENT_ID, …)
addClaviOpenIdConnect(app, (c) => c
  .readFrom.env()
  .acceptAnyServerCertificate(process.env.NODE_ENV !== 'production'));

app.get('/catalog/manage', requireClaviAuth(['acme.catalog.write']), (req, res) => {
  res.json({ ok: true, user: req.user });
});
```

Routes added: `/login`, `CallbackPath` (default `/signin-oidc`), `/logout`, `SignedOutCallbackPath`, `/me`.  
Admin client: `app.locals.claviAdmin`.
