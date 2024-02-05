#!/bin/sh

docker compose -f /home/estr/HomeServerUtils/docker-compose.yaml run rclone sync -v yandex:Фотокамера/Ника /mount/photos
docker compose -f /home/estr/HomeServerUtils/docker-compose.yaml run converter
docker compose -f /home/estr/HomeServerUtils/docker-compose.yaml exec photoprism photoprism index --cleanup
