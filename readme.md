## Fix Cyrilic

```sh
sudo dpkg-reconfigure console-setup
sudo dpkg-reconfigure locales
```

## Install GH

https://github.com/cli/cli/blob/trunk/docs/install_linux.md

```shell
gh auth login
gh repo clone estrelets/HomeServerUtils
```

## Yandex.Disk: rclone

```
# 1. install rclone
sudo -v ; curl https://rclone.org/install.sh | sudo bash

# 2. authorize in yandex
rclone authorize "yandex"

# 3. copy output to configs/rclone/rclone.conf
```

sync directory Yandex.Disk/test to local ./yd/test

```
# to test without modifications:


# run with logs:
rclone sync -v yandex:test ./yd/test
```


## Samba

```bash
sudo mkdir /share
sudo chown nobody:nogroup /share                                           
sudo chmod 777 /share
sudo mkdir /share/media
sudo chown nobody:nogroup /share/media                                           
sudo chmod 777 /share/media
sudo mkdir /share/configs
sudo chown nobody:nogroup /share/configs                                           
sudo chmod 777 /share/configs

sudo apt-get install samba -y
sudo cp ./configs/samba/smb.conf /etc/samba
sudo systemctl enable --now smbd
```


## Docker compose

```bash
cp example.env .env
docker compose up
```


## Crontab

```
sudo crontab -e

#to end of file:
*/15 * * * * docker compose -f /home/estr/HomeServerUtils/docker-compose.yaml run rclone sync -v yandex:Фотокамера/Ника /mount/media/photos
0 * * * * docker compose -f /home/estr/HomeServerUtils/docker-compose.yaml exec photoprism photoprism index --cleanup
```