# Dockerfile for Agitprop.Web.Client (Angular/SPA served with nginx)

# build stage
FROM node:20-alpine AS build
WORKDIR /app

# install dependencies
COPY Agitprop.Web.Client/package*.json ./
RUN npm ci --no-audit --prefer-offline

# copy sources and build (use repo-relative paths since build context is repo root)
COPY Agitprop.Web.Client/ ./
# try common Angular build commands (some projects use different flags)
RUN npm run build -- --configuration production --output-path=dist || npm run build -- --prod || npm run build

# production stage
FROM nginx:alpine
# copy built static files to nginx www (Angular outputs into dist/<project-name>)
COPY --from=build /app/dist/Agitprop.Web.Client /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
