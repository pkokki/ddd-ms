# Docker Official Images

https://hub.docker.com/_/redis

## How to use this image

### start a redis instance

```$ docker run --name some-redis -p 6379:6379 -d redis```

### start with persistent storage

```$ docker run --name some-redis -p 6379:6379 -d redis redis-server --appendonly yes```

If persistence is enabled, data is stored in the ```VOLUME /data```, which can be used with ```--volumes-from some-volume-container``` or ```-v /docker/host/dir:/data``` (see [docs.docker volumes](https://docs.docker.com/engine/tutorials/dockervolumes/)).

For more about Redis Persistence, see http://redis.io/topics/persistence.

### to connect to the Redis CLI

```docker exec -it <container name> redis-cli```