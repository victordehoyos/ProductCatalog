# Etapa de construcción
FROM node:18-alpine AS builder
WORKDIR /app

# Copiar package.json e instalar dependencias
COPY package*.json ./
RUN npm ci

# Copiar código fuente y construir
COPY . .
RUN npm run build

# Etapa de producción
FROM node:18-alpine AS runner
WORKDIR /app

# Crear usuario no-root por seguridad
RUN addgroup -g 1001 -S nodejs
RUN adduser -S nextjs -u 1001

# Copiar desde la etapa de builder
COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next ./.next
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/package.json ./package.json

USER nextjs

EXPOSE 3000

ENV PORT 3000
ENV HOSTNAME "0.0.0.0"

CMD ["npm", "start"]