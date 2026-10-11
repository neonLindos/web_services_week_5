.DEFAULT_GOAL := homework

.PHONY: homework lab prac check

homework lab prac:
	$(MAKE) -C $@ run

check:
	dotnet run --project checks/checks.csproj
