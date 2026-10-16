FROM node:20-alpine AS build
WORKDIR /app
COPY taskboard-web/package*.json ./
RUN npm ci
COPY taskboard-web/ ./
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
