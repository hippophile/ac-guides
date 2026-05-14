.PHONY: web cli build eval

## Start the Blazor web app (auto-opens browser at http://localhost:5050)
## Start the Blazor web app with hot-reload (auto-restarts on code changes)
web:
	dotnet watch --project src/Agile.Web run

## Build the whole solution
build:
	dotnet build src/Agile.sln

## Run the CLI (pass ARGS= to add flags)
## Example: make cli ARGS="run eval --dataset datasets/golden/chatbot-sample.yaml --model gpt-4o-mini"
cli:
	dotnet run --project src/Agile.Cli -- $(ARGS)

## Quick eval shortcut against the chatbot sample dataset
eval:
	dotnet run --project src/Agile.Cli -- run eval \
		--dataset datasets/golden/chatbot-sample.yaml \
		--model copilot:gpt-4.1 \
		--delay 500 \
		--reports-dir reports \
		--template templates/report.scriban.html
