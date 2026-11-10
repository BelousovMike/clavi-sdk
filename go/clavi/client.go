package clavi

// Client is the Admin API entry point.
//
//	c, err := clavi.New(clavi.Config{BaseURL: "https://clavi.example/"})
//	if err != nil { ... }
//	if err := c.Auth("admin", password); err != nil { ... }
//	page, err := c.Users.Find(1, 20, "")
type Client struct {
	http     *httpClient
	Users    *UsersResource
	Groups   *GroupsResource
	Roles    *RolesResource
	Clients  *ClientsResource
	Audit    *AuditResource
	Tenants  *TenantsResource
	Security *SecurityResource
	Me       *MeResource
}

// New builds a client. Call Auth before using the resource methods.
func New(cfg Config) (*Client, error) {
	normalized, err := cfg.withDefaults()
	if err != nil {
		return nil, err
	}
	c := &Client{http: newHTTP(normalized)}
	c.Users = &UsersResource{c: c}
	c.Groups = &GroupsResource{c: c}
	c.Roles = &RolesResource{c: c}
	c.Clients = &ClientsResource{c: c}
	c.Audit = &AuditResource{c: c}
	c.Tenants = &TenantsResource{c: c}
	c.Security = &SecurityResource{c: c}
	c.Me = &MeResource{c: c}
	return c, nil
}

// Auth logs in through the BFF and keeps the session cookie on this client.
func (c *Client) Auth(username, password string) error {
	return c.http.post("api/v1/auth/login", map[string]string{
		"username": username,
		"password": password,
	}, nil)
}

// Logout ends the BFF session.
func (c *Client) Logout() error {
	return c.http.post("api/v1/auth/logout", nil, nil)
}
