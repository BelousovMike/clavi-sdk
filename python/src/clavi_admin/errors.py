class ClaviApiError(RuntimeError):
    def __init__(self, status: int, code: str | None, body: str):
        self.status = status
        self.code = code
        self.body = body
        truncated = body if len(body) <= 200 else body[:200]
        msg = f"Clavi HTTP {status}" + (f" [{code}]" if code else "") + f": {truncated}"
        super().__init__(msg)
