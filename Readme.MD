1) CD INTO CLONED DIRECTORY
2) docker compose up --build
3) Wait until all services started
4) Open http://localhost:8080/swagger/index.html
5) Execute. POST /cowboys ENDPOINT // AND COPY GUID
6) To get result. Execute GET /cowboys/results/game/{gameId} endpoint with copied GUID