package clavi

type PagedResult[T any] struct {
	Items    []T `json:"items"`
	Total    int `json:"total"`
	Page     int `json:"page"`
	PageSize int `json:"pageSize"`
}

type UserResponse struct {
	ID        string `json:"id"`
	TenantID  string `json:"tenantId"`
	Username  string `json:"username"`
	CreatedAt string `json:"createdAt"`
}

type GroupResponse struct {
	ID          string `json:"id"`
	Name        string `json:"name"`
	Description string `json:"description"`
	ParentID    string `json:"parentId"`
	Path        string `json:"path"`
	MemberCount int    `json:"memberCount"`
	CreatedAt   string `json:"createdAt"`
}

type RoleResponse struct {
	ID          string `json:"id"`
	TenantID    string `json:"tenantId"`
	Name        string `json:"name"`
	Description string `json:"description"`
	IsSystem    bool   `json:"isSystem"`
	CreatedAt   string `json:"createdAt"`
}

type ClientResponse struct {
	ID          string `json:"id"`
	TenantID    string `json:"tenantId"`
	ClientID    string `json:"clientId"`
	Name        string `json:"name"`
	RequirePkce bool   `json:"requirePkce"`
	IsEnabled   bool   `json:"isEnabled"`
	CreatedAt   string `json:"createdAt"`
}

type TenantResponse struct {
	ID        string `json:"id"`
	Name      string `json:"name"`
	Slug      string `json:"slug"`
	State     string `json:"state"`
	CreatedAt string `json:"createdAt"`
}

type MeProfileResponse struct {
	ID            string   `json:"id"`
	TenantID      string   `json:"tenantId"`
	Username      string   `json:"username"`
	Email         string   `json:"email"`
	EmailVerified bool     `json:"emailVerified"`
	MfaEnabled    bool     `json:"mfaEnabled"`
	CreatedAt     string   `json:"createdAt"`
	Roles         []string `json:"roles"`
}

type PasswordPolicyResponse struct {
	MinLength          int  `json:"minLength"`
	RequireUppercase   bool `json:"requireUppercase"`
	RequireLowercase   bool `json:"requireLowercase"`
	RequireDigit       bool `json:"requireDigit"`
	RequireSpecial     bool `json:"requireSpecial"`
	HistorySize        int  `json:"historySize"`
	MaxAgeDays         int  `json:"maxAgeDays"`
	BreachCheckEnabled bool `json:"breachCheckEnabled"`
}

type AuditEntryHttpItem struct {
	ID           string `json:"id"`
	EventType    string `json:"eventType"`
	Message      string `json:"message"`
	ActorID      string `json:"actorId"`
	Action       string `json:"action"`
	ResourceType string `json:"resourceType"`
	ResourceID   string `json:"resourceId"`
	CreatedAt    string `json:"createdAt"`
}
