#!/bin/sh

docker compose -f /home/estr/HomeServerUtils/docker-compose.yaml run rclone sync -v yandex:Фотокамера/Ника /mount/photos