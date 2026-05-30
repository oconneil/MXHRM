FROM node:24-alpine AS build
WORKDIR /app

COPY client/mxhrm-web/package*.json ./
RUN npm ci

COPY client/mxhrm-web/ ./
RUN npm run build

FROM nginx:1.29-alpine AS runtime

COPY client/mxhrm-web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/mxhrm-web/browser /usr/share/nginx/html

EXPOSE 80