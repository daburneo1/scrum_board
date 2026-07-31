FROM node:20-alpine AS build

WORKDIR /app

COPY package.json package-lock.json ./

RUN npm ci

COPY . .

RUN npm run build && \
    mkdir -p /app/publish && \
    BUILD_DIRECTORY="$(dirname "$(find dist -name index.html -print -quit)")" && \
    test -n "$BUILD_DIRECTORY" && \
    cp -r "$BUILD_DIRECTORY"/. /app/publish/

FROM nginx:1.27-alpine AS final

COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /app/publish/ /usr/share/nginx/html/

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
