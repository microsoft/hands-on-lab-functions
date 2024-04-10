
```bash
# Create a folder for your function app and navigate to it
mkdir <function-app-name>
cd <function-app-name>

# Create the new function app as a .NET 8 Isolated project
# No need to specify a name, the folder name will be used by default
func init FuncDurable --worker-runtime dotnetIsolated --target-framework net8.0

cd FuncDurable

func new --name HelloOrchestration --template "DurableFunctionsOrchestration" --namespace FuncDurable

# Open the new projet inside VS Code
code .

```