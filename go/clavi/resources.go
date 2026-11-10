package clavi

// UsersResource is /api/v1/users.
type UsersResource struct{ c *Client }

// Find returns a paged user list. Pass search to filter; empty string skips it.
func (r *UsersResource) Find(page, pageSize int, search string) (*PagedResult[UserResponse], error) {
	var out PagedResult[UserResponse]
	err := r.c.http.get(pagedPath("api/v1/users", page, pageSize, search), &out)
	return &out, err
}

func (r *UsersResource) FindOne(id string) (*UserResponse, error) {
	var out UserResponse
	err := r.c.http.get("api/v1/users/"+id, &out)
	return &out, err
}

func (r *UsersResource) Create(username, password, email string) (*UserResponse, error) {
	body := map[string]any{"username": username, "password": password}
	if email != "" {
		body["email"] = email
	}
	var out UserResponse
	err := r.c.http.post("api/v1/users", body, &out)
	return &out, err
}

func (r *UsersResource) Delete(id string) error {
	return r.c.http.delete("api/v1/users/" + id)
}

// GroupsResource is /api/v1/groups.
type GroupsResource struct{ c *Client }

func (r *GroupsResource) Find(page, pageSize int, search string) (*PagedResult[GroupResponse], error) {
	var out PagedResult[GroupResponse]
	err := r.c.http.get(pagedPath("api/v1/groups", page, pageSize, search), &out)
	return &out, err
}

func (r *GroupsResource) Create(name string) (*GroupResponse, error) {
	var out GroupResponse
	err := r.c.http.post("api/v1/groups", map[string]any{"name": name}, &out)
	return &out, err
}

func (r *GroupsResource) Delete(id string) error {
	return r.c.http.delete("api/v1/groups/" + id)
}

// RolesResource is /api/v1/roles.
type RolesResource struct{ c *Client }

func (r *RolesResource) Find() ([]RoleResponse, error) {
	var out []RoleResponse
	err := r.c.http.get("api/v1/roles", &out)
	return out, err
}

// ClientsResource is /api/v1/oidc/clients.
type ClientsResource struct{ c *Client }

func (r *ClientsResource) Find(page, pageSize int, search string) (*PagedResult[ClientResponse], error) {
	var out PagedResult[ClientResponse]
	err := r.c.http.get(pagedPath("api/v1/oidc/clients", page, pageSize, search), &out)
	return &out, err
}

// AuditResource is /api/v1/audit/entries.
type AuditResource struct{ c *Client }

func (r *AuditResource) Find(page, pageSize int) (*PagedResult[AuditEntryHttpItem], error) {
	var out PagedResult[AuditEntryHttpItem]
	err := r.c.http.get(pagedPath("api/v1/audit/entries", page, pageSize, ""), &out)
	return &out, err
}

// TenantsResource is /api/v1/system/tenants (platform admin).
type TenantsResource struct{ c *Client }

func (r *TenantsResource) Find(page, pageSize int) (*PagedResult[TenantResponse], error) {
	var out PagedResult[TenantResponse]
	err := r.c.http.get(pagedPath("api/v1/system/tenants", page, pageSize, ""), &out)
	return &out, err
}

// SecurityResource covers tenant security policies.
type SecurityResource struct{ c *Client }

func (r *SecurityResource) GetPasswordPolicy() (*PasswordPolicyResponse, error) {
	var out PasswordPolicyResponse
	err := r.c.http.get("api/v1/security/policies/password", &out)
	return &out, err
}

// MeResource is the current session profile.
type MeResource struct{ c *Client }

func (r *MeResource) Get() (*MeProfileResponse, error) {
	var out MeProfileResponse
	err := r.c.http.get("api/v1/me", &out)
	return &out, err
}
