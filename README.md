docker build . -t playground

docker stack deploy -c swarm.yml demo

docker service scale demo_playground=0

node install
node wsserver.js