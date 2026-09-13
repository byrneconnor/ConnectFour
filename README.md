# ConnectFour application

## Backend API

To run the backend API locally:
* Run this command in a bash window: `dotnet run --project src/ConnectFour.Api`. This locally runs the Vite dev-server origin (http://localhost:5173). 
* In a separate bash window, requests could be sent to the API to test the correct responses were being received. To initialise a game, the following command can be run: `curl -i -X POST http://localhost:5291/api/games`
* To get an existing games current state, the following command could be run: `curl -i http://localhost:5291/api/games/PASTE_ID_HERE`
* To play a move, the following command could be run: `curl -i -X POST http://localhost:5291/api/games/PASTE_ID_HERE/moves -H "Content-Type: application/json" -d '{"column": 3}'`

## Frontend UI

To run the frontend locally:
* Ensure the backend API is running locally
* Navigate to the frontend root: `cd src/ConnectFour.Web`
* Run `npm run dev` 
* Open http://localhost:5173

## Evaluation

All aspects of the evaluation phase can be run from bash using the following command: 

`dotnet run --project src/ConnectFour.Evaluation -- <ADD COMMAND HERE>`

You must add the command related to the phase you want to run:
* `--web-scrape`: Scrape benchmark positions from the [online solver](https://connect4.gamesolver.org/)
* `--split`: Produce the train/test split
* `--benchmark-evaluation-train`: Tune configurations on the train split
* `--benchmark-evaluation-test`: Evaluate final configurations on the test split
* `--arena-evaluation`: Run the AI vs AI arena evaluation