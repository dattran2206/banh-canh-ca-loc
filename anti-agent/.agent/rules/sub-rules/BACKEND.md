# BACKEND & DATABASE RULES - Antigravity Kit

## API Standards
- **REST**: Semantic URL naming, standard HTTP codes (200, 201, 400, 401, 403, 404, 500).
- **Versioning**: Header-based or URL-based (/v1/).
- **Validation**: Strict schema validation for all requests.

## 🗄️ Database Patterns
- **Prisma**: Always use migrations. No manual DB changes.
- **Drizzle**: Schema-first approach.
- **Indexing**: Mandatory for foreign keys and searchable fields.

## 🔐 Security
- **Auth**: JWT or Session-based (HttpOnly cookies).
- **Sanitization**: All inputs must be sanitized to prevent SQLi/XSS.
- **Secrets**: Never hardcode. Use .env and process.env.

---

## CHECKLIST
- [ ] Error handling on all async calls.
- [ ] Database transactions for multi-step ops.
- [ ] Input validation (Zod/Joi).
