package clavi

import "fmt"

type APIError struct {
	Status int
	Code   string
	Body   string
}

func (e *APIError) Error() string {
	body := e.Body
	if len(body) > 200 {
		body = body[:200]
	}
	if e.Code == "" {
		return fmt.Sprintf("Clavi HTTP %d: %s", e.Status, body)
	}
	return fmt.Sprintf("Clavi HTTP %d [%s]: %s", e.Status, e.Code, body)
}
