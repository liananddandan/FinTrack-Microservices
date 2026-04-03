FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
COPY apps ./apps

RUN npm install
RUN npm run build -w apps/web-portal-react

FROM nginx:alpine
COPY apps/web-portal-react/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/apps/web-portal-react/dist /usr/share/nginx/html
EXPOSE 80