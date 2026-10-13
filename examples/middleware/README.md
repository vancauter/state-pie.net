# Middleware

Shows synchronous, typed middleware around dispatch. The middleware logs before and after forwarding each action; it
calls `next` exactly once and lets exceptions propagate.
