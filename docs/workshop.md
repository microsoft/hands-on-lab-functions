---
published: true
type: workshop
title: Product Hands-on Lab - Functions
short_title: Azure Functions
description: This workshop will cover Azure Functions service that you will use to build a complete real world scenario.
level: beginner # Required. Can be 'beginner', 'intermediate' or 'advanced'
authors: # Required. You can add as many authors as needed
  - Damien Aicheh
  - Iheb Khemissi
contacts: # Required. Must match the number of authors
  - "@damienaicheh"
  - "@ikhemissi"
duration_minutes: 180
tags: azure, azure functions, azure durable functions, event grid, key vault, cosmos db, web pubsub, static web app, csu, codespace, devcontainer
navigation_levels: 3
---

# Product Hands-on Lab - Functions

Welcome to this Azure Functions Workshop. You'll be experimenting with Azure Functions service in multiple labs to achieve a real world scenario. Don't worry, even if the challenges will increase in difficulty, this is a step by step lab, you will be guided through the whole process.

During this workshop you will have the instructions to complete each steps. It is recommended to search for the answers in provided resources and links before looking at the solutions placed under the '📚 Toggle solution' panel.

<div class="task" data-title="Task">

> You will find the instructions and expected configurations for each Lab step in these yellow **TASK** boxes.
> Inputs and parameters to select will be defined, all the rest can remain as default as it has no impact on the scenario.
>
> Log into your Azure subscription locally using Azure CLI and on the [Azure Portal][az-portal] using your own credentials.
> Instructions and solutions will be given for the Azure CLI, but you can also use the Azure Portal if you prefer.

</div>

## 🚀 Dev Environment Setup

Before starting this lab, be sure to set your Azure environment :

- An Azure Subscription with the **Contributor** role to create and manage the labs' resources and deploy the infrastructure as code
- A dedicated resource group for this lab to ease the cleanup at the end.
- Register the Azure providers on your Azure Subscription if not done yet: `Microsoft.CognitiveServices`, `Microsoft.DocumentDB`, `Microsoft.KeyVault`, `Microsoft.SignalRService`, `Microsoft.Web`, `Microsoft.ApiManagement`.


To retrieve the lab content :

- A Github account (Free, Team or Enterprise)
- Create a [fork][Repo-fork] of the repository from the **main** branch to help you keep track of your changes

3 development options are available:
  - 🥇 **Preferred method** : Pre-configured GitHub Codespace 
  - 🥈 Local Devcontainer
  - 🥉 Local Dev Environment with all the prerequisites detailed below

<div class="tip" data-title="Tips">

> To focus on the main purpose of the lab, we encourage the usage of devcontainers/codespace as they abstract the dev environment configuration, and avoid potential local dependencies conflict.
> 
> You could decide to run everything without relying on a devcontainer : To do so, make sure you install all the prerequisites detailed below.

</div>

### 🥇 : Pre-configured GitHub Codespace

To use a Github Codespace, you will need :
- [A GitHub Account][github-account]

Github Codespace offers the ability to run a complete dev environment (Visual Studio Code, Extensions, Tools, Secure port forwarding etc.) on a dedicated virtual machine. 
The configuration for the environment is defined in the `.devcontainer` folder, making sure everyone gets to develop and practice on identical environments : No more conflict on dependencies or missing tools ! 

Every Github account (even the free ones) grants access to 120 vcpu hours per month, _**for free**_. A 2 vcpu dedicated environment is enough for the purpose of the lab, meaning you could run such environment for 60 hours a month at no cost!

To get your codespace ready for the labs, here are a few steps to execute : 
- After you forked the repo, click on `<> Code`, `Codespaces` tab and then click on the `+` button:

![codespace-new](./assets/codespace-new.png)

- You can also provision a beefier configuration by defining creation options and select the **Machine Type** you like: 

![codespace-configure](./assets/codespace-configure.png)

### 🥈 : Using a local Devcontainer

This repo comes with a Devcontainer configuration that will let you open a fully configured dev environment from your local Visual Studio Code, while still being completely isolated from the rest of your local machine configuration : No more dependancy conflict.
Here are the required tools to do so : 

- [Git client][git-client] 
- [Docker Desktop][docker-desktop] running
- [Visual Studio Code][vs-code] installed

Start by cloning the Hands-on-lab-Functions repo you just forked on your local Machine and open the local folder in Visual Studio Code.
Once you have cloned the repository locally, make sure Docker Desktop is up and running and open the cloned repository in Visual Studio Code.  

You will be prompted to open the project in a Dev Container. Click on `Reopen in Container`. 

If you are not prompted by Visual Studio Code, you can open the command palette (`Ctrl + Shift + P`) and search for `Reopen in Container` and select it: 

![devcontainer-reopen](./assets/devcontainer-reopen.png)

### 🥉 : Using your own local environment

The following tools and access will be necessary to run the lab in good conditions on a local environment :  

- [Git client][git-client] 
- [Visual Studio Code][vs-code] installed (you will use Dev Containers)
- [Azure CLI][az-cli-install] installed on your machine
- [Azure Functions Core Tools][az-func-core-tools] installed, this will be useful for creating the scaffold of your Azure Functions using command line.
- If you are using VS Code, you can also install the [Azure Function extension][azure-function-vs-code-extension]
- The following languages if you want to run all the Azure Functions solutions : 
  - [.Net 8][download-dotnet]

Once you have set up your local environment, you can clone the Hands-on-lab-Functions repo you just forked on your machine, and open the local folder in Visual Studio Code and head to the next step. 

## 🚀 Visual Studio Code Setup

### 👉 Load the Workspace

Once your environment is ready, you will have to enter the Visual Studio Workspace to get all the tools ready.
To do so, click the **burger menu** in the top left corner (visible only with codespace), **File** and then **Open Workspace from File...** 

![codespace-workspace](./assets/codespace-workspace.png)

- Select `.vscode/hands-on-lab-functions.code-workspace` :

![codespace-workspace-select](./assets/codespace-workspace-select.png)

- You are now ready to go! For the rest of the lab, in case you lose the terminal, you can press `Ctrl + J` or open a new one here : 

![codespace-terminal-new](./assets/codespace-terminal-new.png)

Let's begin!

### 🔑 Sign in to Azure

<div class="task" data-title="Task">

> - Log into your Azure subscription in your environment using Azure CLI and on the [Azure Portal][az-portal] using your credentials.
> - Instructions and solutions will be given for the Azure CLI, but you can also use the Azure Portal if you prefer.

</div>

<details>

<summary>📚 Toggle solution</summary>

```bash
# Login to Azure : 
# --tenant : Optional | In case your Azure account has access to multiple tenants

# Option 1 : Local Environment 
az login --tenant <yourtenantid or domain.com>
# Option 2 : Github Codespace : you might need to specify --use-device-code parameter to ease the az cli authentication process
az login --use-device-code --tenant <yourtenantid or domain.com>

# Display your account details
az account show
# Select your Azure subscription
az account set --subscription <subscription-id>

# Register the following Azure providers if they are not already

# Azure Cognitive Services
az provider register --namespace 'Microsoft.CognitiveServices'
# Azure CosmosDb
az provider register --namespace 'Microsoft.DocumentDB'
# Azure Key Vault
az provider register --namespace 'Microsoft.KeyVault'
# Azure Web PubSub
az provider register --namespace 'Microsoft.SignalRService'
# Azure API Management
az provider register --namespace 'Microsoft.ApiManagement'
# Azure Functions
az provider register --namespace 'Microsoft.Web'
```

</details>

### Deploy the infrastructure

You must deploy the infrastructure before starting the lab. 

First, you need to initialize the terraform infrastructure by running the following command:

```bash
cd terraform && terraform init
```

Then run the following command to deploy the infrastructure:

```bash
# Apply the deployment directly
terraform apply -auto-approve
```

Now you can deploy the web app code into the Static Web App:

```bash
# Restore packages 
cd ../src/webapp && npm install

# Build the Web App
npm run swa:build

# Deploy the web app code into the Static Web App
RESOURCE_GROUP_NAME="$(terraform output -raw resource_group_name)"
STATIC_WEB_APP="$(terraform output -raw static_web_app_name)"

npm run swa:deploy -- \
  --resource-group $RESOURCE_GROUP_NAME \
  --app-name $STATIC_WEB_APP \
  --no-use-keychain
```

The deployment should take around 5 minutes to complete.

## Scenario

The goal of the full lab is to upload an audio file to Azure and retrieve the transcripts back using a Web Application.

Here is a diagram to illustrate the flow:

![Hand's On Lab Architecture](assets/architecture-overview.svg)

1. You will open the demo web application which sends an HTTP GET request to APIM (API Management) to fetch existing transcriptions. APIM will be used as a facade for multiple APIs.
1. The request is forwarded from APIM to an Azure Function endpoint handling transcription fetching
1. This Azure Function endpoint retrieves the latest transcriptions stored in Cosmos DB, and returns them to the web application
1. You will then upload an [audio file](assets/whatstheweatherlike.wav) in the web application interface and the web application sends an HTTP request to APIM
1. An Azure Function endpoint handling uploads will process the request 
1. The Azure Function will upload the file to a Storage Account
1. When the file is uploaded an Azure Durable Function will detect it and start processing it
1. The audio file is sent to Azure Cognitive Services via the Azure Durable Function. The speech to text cognitive service will process the file and return the result to the Azure Durable Function.
1. The Azure Durable Function will then store the transcript of the audio file in a Cosmos DB Database
1. Another Azure Function endpoint will be triggered by the update event in CosmosDB.
1. The Azure Function will then fetch the transcript from CosmosDB and publish it to Web Pub/Sub
1. The web application being a subscriber of the Web Pub-Sub resource, it will be notified about the new transcript being added via a websocket and display it in the list.

<div class="info" data-title="Note">

> Azure Key Vault will be used to secure the secrets used through the entire scenario.

</div>

You will get more details about each of these services during the Hands On Lab.

## Programming language

You will have to create few functions in this workshop to address our overall scenario. You can choose the programming language you are the most comfortable with among the ones [supported by Azure Functions][az-func-languages]. We will provide examples in .NET 8 (isolated) for the moment, but other languages might be added in the future.

With everything ready let's start the lab 🚀

[az-cli-install]: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli
[az-func-core-tools]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Clinux%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools
[az-func-languages]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-versions#languages
[az-portal]: https://portal.azure.com
[vs-code]: https://code.visualstudio.com/
[azure-function-vs-code-extension]: https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions
[docker-desktop]: https://www.docker.com/products/docker-desktop/
[Repo-fork]: https://github.com/microsoft/hands-on-lab-functions/fork
[git-client]: https://git-scm.com/downloads
[github-account]: https://github.com/join
[download-dotnet]: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

---

# Lab 1 : Transcribe an audio file (2 hours)

For this first lab, you will focus on the following scope :

![Hand's On Lab Architecture Lab 1](assets/architecture-lab1.svg)

The Azure storage account is used to store data objects, including blobs, file shares, queues, tables, and disks. You will use it to store the audios files inside an `audios` container.

To check that everything was created as expected, open the [Azure Portal][az-portal] and you should retrieve your `audios` container:

![Storage account access keys](assets/storage-account-show-container.png)


## Upload a file (30 min)

### Azure Functions : A bit of theory

Azure Functions is a `compute-on-demand` solution, offering a common function programming model for various languages. To use this serverless solution, no need to worry about deploying and maintaining infrastructures, Azure provides with the necessary up-to-date compute resources needed to keep your applications running. Focus on your code and let Azure Functions handle the rest.

Azure Functions are event-driven : They must be triggered by an event coming from a variety of sources. This model is based on a set of `triggers` and `bindings` which let you avoid hardcoding access to other services. Your function receives data (for example, the content of a queue message) in function parameters. You send data (for example, to create a queue message) by using the return value of the function :

- `Binding` to a function is a way of declaratively connecting another resource to the function; bindings may be connected as input bindings, output bindings, or both. Azure services such as Azure Storage blobs and queues, Service Bus queues, Event Hubs, and Cosmos DB provide data to the function as parameters.
- `Triggers` are a specific kind of binding that causes a function to run. A trigger defines how a function is invoked, and a function must have exactly one trigger. Triggers have associated data, which is often provided as a parameter payload to the function.

In the same `Function App` you will be able to add multiple `functions`, each with its own set of triggers and bindings. These triggers and bindings can benefit from existing `expressions`, which are parameter conventions easing the overall development experience. For example, you can use an expression to use the execution timestamp, or generate a unique `GUID` name for a file uploaded to a storage account.

Azure Functions run and benefit from the App Service platform, offering features like: deployment slots, continuous deployment, HTTPS support, hybrid connections and others. Apart from the `Consumption` (Serverless) model we're most interested in this Lab, Azure Functions can also be deployed a dedicated `App Service Plan`or in a hybrid model called `Premium Plan`.

### Azure Functions : Let's practice

At this stage in our scenario, the goal is to upload an audio into the Storage Account inside the `audios` container. To achieve this, an Azure Function will be used as an API to upload the audio file with a unique `GUID` name to your storage account.

<div class="task" data-title="Tasks">

> Create an `Azure Function` with a POST `HTTP Trigger` and a `Blob Output Binding` to upload the file to the storage account. The Blob Output Binding will use a `binding expression` to generate a unique `GUID` name for the file.
>
> Use the `func` CLI tool and .NET 8 using the isolated mode to create this Azure Function

</div>

<div class="tip" data-title="Tips">

> An Azure Function example solution will be provided below in .NET 8.
>
> [Azure Functions][azure-function]<br> 
> [Azure Function Core Tools][azure-function-core-tools]<br> 
> [Basics of Azure Functions][azure-function-basics]<br> 
> [HTTP Triggered Azure Function][azure-function-http]<br>
> [Blob Output Binding][azure-function-blob-output]<br> 
> [Azure Functions Binding Expressions][azure-function-bindings-expression]

</div>

<details>
<summary>📚 Toggle solution</summary>

If necessary the source code with the solutions can be found in this Github Repository, under `./src/solutions/FuncStd`.

#### Preparation

You will create a function using the [Azure Function Core Tools][azure-function-core-tools]:

```bash
# Create a folder for your function app and navigate to it
mkdir <function-app-name>
cd <function-app-name>

# Create the new function app as a .NET 8 Isolated project
# No need to specify a name, the folder name will be used by default
func init --worker-runtime dotnet-isolated --target-framework net8.0

# Create a new function endpoint with an HTTP trigger to which you'll be able to send the audio file
func new --name AudioUpload --template 'HTTP Trigger'

# Add a new Nuget package dependency to the Blob storage SDK
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.Storage.Blobs --version 6.3.0

# Open the new projet inside VS Code
code .

```

If you open the Azure Function App resource started with `func-std` in the [Azure Portal][az-portal] and go to the `Environment variables` panel. You should see in App Settings the `STORAGE_ACCOUNT_CONTAINER` set to `audios` and the connection string of the storage account already pre-populated in the `STORAGE_ACCOUNT_CONNECTION_STRING` environment variable.

Finally, make sure to set `FILE_UPLOADING_FORMAT` to `binary` in the Static Web App settings as this function implementation expects the audio file contents to be passed directly in the POST request body, without using a form.

#### .NET 8 implementation

In this version of the implementation, you will be using the [.NET 8 Isolated](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-in-process-differences) runtime.

Now that you have a skeleton for our `AudioUpload` function in the `AudioUpload.cs` file, you will need to update it to meet the following goals:

- It should read the uploaded file from the body of the POST request
- It should store the file as a blob inside the blob Storage Account
- It should respond to user with a status code 200

To upload the file, you will rely on the blob output binding [`BlobOutput`](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob-output?tabs=python-v2%2Cin-process&pivots=programming-language-csharp) of the Azure Function, which will take care of the logic of connecting to the Storage Account and uploading the function with minimal line of code in our side.

To do this, let's start by adding a `AudioUploadOutput` class to the `AudioUpload.cs` file for simplicity but it can be done in a specific class.

```csharp
public class AudioUploadOutput
{
    [BlobOutput("%STORAGE_ACCOUNT_CONTAINER%/{rand-guid}.wav", Connection="STORAGE_ACCOUNT_CONNECTION_STRING")]
    public byte[] Blob { get; set; }

    public HttpResponseData HttpResponse { get; set; }
}
```

This class will handle uploading the blob and returning the HTTP response:

- The blob will be stored in the container identified by `STORAGE_ACCOUNT_CONTAINER` which is an environment variable.
- The blob will be named `{rand-guid}.wav` which resolves to a UUID followed by `.wav`.
- `STORAGE_ACCOUNT_CONNECTION_STRING` is the name of App setting which contains the connection string that you will use to connect to the blob storage account

Next, you will need to update the class `AudioUpload` to add the logic for reading the file from the request, and then use `AudioUploadOutput` to perform the blob upload and returning the response.

Update the code of the `Run` method in the `AudioUpload` class as follows:

```csharp
[Function(nameof(AudioUpload))]
public AudioUploadOutput Run(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req
)
{
    _logger.LogInformation("C# HTTP trigger function processed a request.");

    // Read the file contents from the request
    // and store it in the `audioFileData` buffer
    var audioFileData = default(byte[]);
    using (var memstream = new MemoryStream())
    {
        req.Body.CopyTo(memstream);
        audioFileData = memstream.ToArray();
    }

    // Prepare the response to return to the user
    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
    response.WriteString("Uploaded!");

    // Use AudioUploadOutput to return the response and store the blob
    return new AudioUploadOutput()
    {
        Blob = audioFileData,
        HttpResponse = response
    };
}
```

</details>

#### Deployment and testing

##### Option 1 : Deploy your function with VS Code

- Open the Azure extension in VS Code left panel
- Make sure you're signed in to your Azure account
- Open the Function App panel
- Right-click on your function app and select `Deploy to Function App...`

![Deploy to Function App](assets/function-app-deploy.png)

##### Option 2 : Deploy your function with the Azure Function Core Tools

Deploy your function using the VS Code extension or by command line:

```bash
func azure functionapp publish 
func-std-<your-instance-suffix-name>
```

Let's give a try using Postman. Go to the Azure Function and select `Functions` then `AudioUpload` and select the `Get Function Url` with the `default (function key)`. 
The Azure Function url is protected by a code to ensure a basic security layer. 

![Azure Function url credentials](assets/func-url-credentials.png)

Use this url into your Postman to upload the audio file. Create a POST request and in the row where you set the key to `audios` make sure to select the file option in the hidden dropdown menu to be able to select a file in the value field:

![Postman](assets/func-postman.png)

</details>

### Connect the Web App

It's now time to connect the Azure Function App which stand for a small API to upload your audio file and the Static Web App which is the front end of your application. The API Management (APIM) will be added in a future lab.

<div class="task" data-title="Task">

> - Check the README of the web app project to explore the environment variables supported by the Web App you deployed on Lab 0 and set the value of `FILE_UPLOADING_URL` to the url of the uploaded function which you have created in the previous lab.
> - If you followed the solutions provided above to create your **AudioUpload** Function endpoint: Set `FILE_UPLOADING_FORMAT` to `binary`, as it will force the frontend to treat the Azure Function endpoint as a binary receiver, rather than a multi-part form receiver.

</div>

<details>
<summary>📚 Toggle solution</summary>

First, go to the Azure Static Web App resource and inside `Configuration` in the `Application Settings` set the environment variable `FILE_UPLOADING_URL` to the same Azure Function `AudioUpload` endpoint you retrieved earlier in the lab above like `https://<functionapp>.azurewebsites.net/api/audioupload?code=<...>`.

If your function expects a binary as an input then you will also need to set `FILE_UPLOADING_FORMAT` to `binary`

Now if you try to upload a file using the Web App interface you should see a green box in the bottom left corner with a success message.

![Web App](assets/static-web-app-upload-succeeded.png)

</details>

## Process the audio file (1 hour 30 min)
 
To process the audio file, extract the transcript and save it to Cosmos DB, you will need to create a Durable Function. Durable Functions are an extension of Azure Functions that lets you write stateful functions in a serverless environment. The extension manages state, checkpoints, and restarts for you.

### Detect a file upload event 

Now you have the audio file uploaded to the storage account, you will need to detect this event to trigger the next steps of the scenario.

<div class="task" data-title="Tasks">

> Create a new `Durable Function` with a `Blob Trigger` to detect the file upload event and start the processing of the audio file.
>
> Use the `func` CLI tool and .NET 8 using the isolated mode to create this Durable Function

</div>

<details>
<summary>📚 Toggle solution</summary>

```bash
# Create a folder for your function app and navigate to it
mkdir <function-app-name>
cd <function-app-name>

# Create the new function app as a .NET 8 Isolated project
# No need to specify a name, the folder name will be used by default
func init FuncDurable --worker-runtime dotnetIsolated --target-framework net8.0

func new --name HelloOrchestration --template "DurableFunctionsOrchestration" --namespace FuncDurable

# Open the new projet inside VS Code
code .

```

This will create a template for Durable Functions. You will need to rename and update the `HelloOrchestration.cs` file to add the logic for detecting the file upload event and starting the processing of the audio file.


TODO: UPDATE the start of the orchestration function to use the BlobTrigger

```csharp
```

</details>

### Consume Speech to Text APIs

The Azure Cognitive Services are cloud-based AI services that give the ability to developers to quickly build intelligent apps thanks to these pre-trained models. They are available through client library SDKs in popular development languages and REST APIs.

Cognitive Services can be categorized into five main areas:

- Decision : Content Moderator provides monitoring for possible offensive, undesirable, and risky content. Anomaly Detector allows you to monitor and detect abnormalities in your time series data.
- Language : Azure Language service provides several Natural Language Processing (NLP) features to understand and analyze text.
- Speech : Speech service includes various capabilities like speech to text, text to speech, speech translation, and many more.
- Vision : The Computer Vision service provides you with access to advanced cognitive algorithms for processing images and returning information.
- Azure OpenAI Service : Powerful language models including the GPT-3, GPT-4, Codex and Embeddings model series for content generation, summarization, semantic search, and natural language to code translation.

To access these APIs, create a `cognitive service` resource in your subscription. This will instantiate a resource with an associated `API Key` necessary to authenticate the API call owner and apply rate and quota limits as per selected pricing tier.

You now want to retrieve the transcript out of the audio file uploaded thanks to the speech to text cognitive service.

![cognitive service flow](assets/cognitive-service-flow.png)

<div class="task" data-title="Tasks">

> To do this, you will have to:
>
> - Retrieve your auto-generated `Api Key` from the Azure Cognitive Service
> - Call the speech to text API

</div>

<div class="important" data-title="Security">

> Remember to store secrets as connection strings and `Api keys` in an Azure Key Vault to manage and secure their access.

</div>

<div class="tip" data-title="Tips">

> [What are Cognitive Services][cognitive-services]<br>
> [Cognitive Service Getting Started][cognitive-service-api]<br> 
> [Create a Key Vault][key-vault]

</div>

<details>
<summary>📚 Toggle solution</summary>

TODO: A update en fonction de l'usage dans AZ function

To allow the Logic App to access the Key Vault, you need to grant access to it. Go to your Logic App and inside the identity tab, turn on the `System Identity`:

![System Identity](assets/logic-app-system-identity.png)

Then in your Key Vault, go to `Access policies` and create a new one, set the Secret access to `Get` and `List`:

![Key Vault Access](assets/key-vault-secret-access.png)

Then search for your logic app.

![Key Vault Access Logic App](assets/key-vault-access-logic-app.png)

Now inside your Key Vault, in the `Secret` section add a new one called `SpeechToTextApiKey` and set a key from the cognitive service.

If you can't add the secrets, this means that you need to give to the account you are using, access to the Key Vault like you did previously with Logic App. So, in your Key Vault, go to `Access policies` and create a new one, set the Secret access to `Get`, `List` and `Set`.

![Key Vault Cognitive Secret](assets/key-vault-cognitive-secret.png)

With all the pre-requisites set (Key Vault **created**, secret Cognitive Service api key **set**, Logic App Managed Identity access to key vault **enabled**), add a new action in your **logic app workflow** by searching for `Key Vault` and then select `Get Secret`. This will load the speech to text API key once.

![Logic App Key Vault Connection](assets/logic-app-key-vault-connection.png)

Select the Key Vault resource and the name of the secret.

![Logic App Get Secret](assets/logic-app-get-secret.png)

Next, add a new action by searching for `Http`, then fill in the different parameters as follows after retrieving the cognitive service endtpoint from the **resource overview**:

![Logic App HTTP Action](assets/logic-app-http-action.png)

Notice the region of your cognitive service account and the language to use are specified in the API Url. All parameters can be found in the default [sample][default-cognitive-sample]

To validate the flow, go to your storage account and delete the audio file from the `audios` container and upload it once again (to trigger the updated logic app).
In the Logic App `Run History`, you should see the transcript of the audio file as a text output from the HTTP call to Speech to Text API.

![Logic App Run History](assets/logic-app-run-history.png)

Select your run and open the `Http` action block to verify the `Outputs` section : 

![Logic App Run History Details](assets/logic-app-run-result.png)

</details>

### Store data to Cosmos DB

Azure Cosmos DB is a fully managed NoSQL database which offers Geo-redundancy and multi-region write capabilities. It currently supports NoSQL, MongoDB, Cassandra, Gremlin, Table and PostgreSQL APIs and offers a serverless option which is perfect for our use case.

Now the cognitive service provided with a transcript of your audio file, you will have to store it in a NoSQL database inside Cosmos DB.

Now the next step is to add another activity in your Azure Durables Function to store the transcript in the Cosmos DB.

<div class="task" data-title="Tasks">

> Store the JSON object in the Cosmos DB container called `audios_transcripts`

</div>

![Cosmos DB flow](assets/logic-app-hol-cosmos-db.png)

<div class="tip" data-title="Tips">

> [Serverless Cosmos DB][cosmos-db]<br>

</div>

<details>
<summary>📚 Toggle solution</summary>

Finally, it's time to compose the document object to insert using JSON and the `dynamic content` from the previous steps. The document should look like this:

```json
{
  "id": <guid-here>,
  "path": <audio-file-storage-account-path>,
  "result": <cognitive-service-text-result>,
  "status": <cognitive-service-status-result>
}
```

</details>

</details>

#### Deployment and testing

Deploy the Azure Durable Function using the same method as before but with the new function name.

```bash
func-drbl-<your-instance-suffix-name>
```

You can now validate the entire workflow : delete and upload once again the audio file. You should see the new item created above in your Cosmos DB container !

## Lab 1 : Summary

By now you should have a solution that :

- Send new audio files added to a blob storage using a first Azure Function, based on a specific file type (.wav only), and destination (`audios` container).
- Will invoke the execution of an Azure Durable Function responsible for retrieving the audio transcription thanks to a Speech to Text (Cognitive Service) call.
- Once the transcription is retrieved, the Azure Durable Function will store this value in a CosmosDB database.

The first Azure Function API created in the Lab offers a first security layer to the solution as it requires a key to be called, as well as makes sure all the files are stores with a uniquely generated name (GUID).

[az-portal]: https://portal.azure.com
[azure-function]: https://learn.microsoft.com/en-us/cli/azure/functionapp?view=azure-cli-latest
[azure-function-core-tools]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash
[azure-function-basics]: https://learn.microsoft.com/en-us/azure/azure-functions/supported-languages
[azure-function-http]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?pivots=programming-language-python&tabs=python-v2%2Cin-process%2Cfunctionsv2
[azure-function-blob-output]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob-output?pivots=programming-language-python&tabs=python-v2%2Cin-process
[azure-function-bindings-expression]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-expressions-patterns
[event-grid-subject-filtering]: https://learn.microsoft.com/en-us/azure/event-grid/event-filtering#subject-filtering
[storage-account]: https://learn.microsoft.com/fr-fr/cli/azure/storage/account?view=azure-cli-latest
[storage-account-container]: https://learn.microsoft.com/fr-fr/cli/azure/storage/container?view=azure-cli-latest
[event-grid-system-topic]: https://learn.microsoft.com/en-us/azure/event-grid/system-topics
[event-grid-topic-subscription]: https://learn.microsoft.com/en-us/cli/azure/eventgrid/system-topic/event-subscription?view=azure-cli-latest
[azure-messaging-services]: https://learn.microsoft.com/en-us/azure/service-bus-messaging/compare-messaging-services
[azure-cli-extension]: https://learn.microsoft.com/en-us/cli/azure/azure-cli-extensions-overview
[cognitive-services]: https://learn.microsoft.com/en-us/azure/cognitive-services/what-are-cognitive-services
[cognitive-services-cli]: https://learn.microsoft.com/en-us/cli/azure/cognitiveservices/account?view=azure-cli-latest
[key-vault]: https://learn.microsoft.com/fr-fr/cli/azure/keyvault?view=azure-cli-latest
[cognitive-service-api]: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/rest-speech-to-text-short#regions-and-endpoints
[default-cognitive-sample]: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/get-started-speech-to-text?tabs=macos%2Cterminal&pivots=programming-language-rest&ocid=AID3051475&WT.mc_id=javascript-76678-cxa#recognize-speech-from-a-file
[cosmos-db]: https://learn.microsoft.com/en-us/azure/cosmos-db/scripts/cli/nosql/serverless

---

# Lab 2 : Retrieve transcriptions (1 hour)

On this second lab, you will focus on getting back the transcriptions of audio files and displaying them on the demo Web App. The front end of the web app is a Single-Page application with static files hosted in an Azure **Static Web App**. The front end will interact with other backend Http Endpoints services and the CosmosDb instance mainly via built-in Azure Function as a **Managed serverless** backend for the Web App.   

Previously processed transcriptions will be retrieved using HTTP GET requests whereas new transcriptions will be retrieved in real-time using websockets.

![Achitecture scope of Lab 2](assets/architecture-lab2.svg)

## Getting transcriptions on-demand (10 min)

First, let's create a function which returns the latest 50 transcriptions from Cosmos DB.

If you open an item in your Cosmos Db by navigating to the `Data Explorer`, you should have at least the following properties:

```typescript
{
    "id": "<item-id>",
    "path": "<audio-path>",
    "result": "<audio-content>",
    "status": "<transcription-status>",
    "_ts": <time-span>
    ...other default properties
}
```

This function will be used to show all existing transcriptions on the demo Web App when you first load it.

<div class="task" data-title="Task">

> Create an HTTP-triggered function which returns transcriptions from Cosmos DB:
>
> - Trigger: `HTTP request`
> - Action: Return the latest `50 transcriptions`, in `JSON` format
> - Transcriptions should include the following props: `id`, `path`, `result`, `status`, and `_ts`
> - Define the `TRANSCRIPTION_FETCHING_URL` to the Url of the `GetTranscription` Function endpoint

</div>

<div class="tip" data-title="Resources">

> [Cosmos DB input binding][cosmosdb-input-binding]<br> 
> [OFFSET LIMIT clause in Cosmos DB][offset-limit-clause-in-cosmosdb]<br> 
> [Cosmos DB connection string][cosmosdb-connection-string]

</div>

<div class="info" data-title="Information">

> You will use the first Azure Function from Lab 1 and add a new function to it instead of creating a brand new Function App for every function. This would allow you to group functions together and ease their management while allowing you to share code between them.
>
> In a real world scenario it can be useful to create one Azure Function for each endpoint to be able to scale independently and to have a better separation of concerns.

</div>

<details>
<summary>📚 Toggle solution</summary>

Add a new HTTP-triggered function `GetTranscriptions` to your Function App and use the following settings:

| App setting                         | Description                     | Value                |
|-------------------------------------|---------------------------------|----------------------|
| COSMOS_DB_DATABASE_NAME             | Name of the Cosmos DB database  | `HolDb`              |
| COSMOS_DB_CONTAINER_ID              | Name of the container in the DB | `audios_transcripts` |
| COSMOS_DB_CONNECTION_STRING_SETTING | Cosmos DB connection string     |      In the Azure Portal                |

Go to the Azure Function in your Azure Portal, inside the `Configuration` and `Application settings` and add the 3 new settings values:

- Add the App settings `COSMOS_DB_DATABASE_NAME` and `COSMOS_DB_CONTAINER_ID` and set their values like defined in the Overview section above 
- Set value of the connection string `COSMOS_DB_CONNECTION_STRING_SETTING` using the `Keys` section of your Cosmos Db resource on Azure.

#### .NET 8 implementation

First of all, let's define a class for transcriptions as described in the task details.

Create a `Transcription.cs` file with the following contents:

```csharp
namespace YOUR_NAMESPACE_HERE
{
    public class Transcription
    {
        public string id { get; set; }
        public string path { get; set; }
        public string result { get; set; }
        public string status { get; set; }
        public int _ts { get; set; }
    }
}
```

Don't forget to set the `namespace` to the one used in the other classes (e.g. `AudioUpload.cs`).

Next, you will create the skeleton of the function using the following commands:

```sh
# Create an HTTP-triggered function called GetTranscriptions
func new --name GetTranscriptions --template "HTTP trigger" --authlevel "function"

# Add the Nuget package of Cosmos DB for Functions
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.CosmosDB --version 4.8.0
```

This will generate a new file `GetTranscriptions.cs` with a `GetTranscriptions` class.

As you will need to return a JSON response, let's start by adding the following dependency at the top of the file:

```csharp
using System.Text.Json;
```

Next, you will have to update the logic of the `Run` method of this new class to fetch transcriptions from Cosmos DB and return it to the user.

```csharp
[Function(nameof(GetTranscriptions))]
public HttpResponseData Run(
    [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
    [CosmosDBInput(
        databaseName: "%COSMOS_DB_DATABASE_NAME%",
        collectionName: "%COSMOS_DB_CONTAINER_ID%",
        ConnectionStringSetting = "COSMOS_DB_CONNECTION_STRING_SETTING",
        SqlQuery = "SELECT * FROM c ORDER BY c._ts DESC OFFSET 0 LIMIT 50")
    ] IEnumerable<Transcription> transcriptions
)
{
    _logger.LogInformation("C# HTTP trigger function processed a request.");

    // Prepare the response to return to the user
    var response = req.CreateResponse(HttpStatusCode.OK);
    response.Headers.Add("Content-Type", "application/json");

    // Serialize the transcriptions which you got from Cosmos DB in the JSON format
    string jsonData = JsonSerializer.Serialize(transcriptions);
    response.WriteString(jsonData);

    return response;
}
```

Notice the usage of the Cosmos DB input binding [`CosmosDBInput`][cosmosdb-input] which simplifies retrieving data from a Cosmos DB collection.

In addition to defining the environment variables such as the name of the database, collection, and the connection string, you have also defined a query (`SqlQuery`) to fetch the last 50 items from the collection.

Check the [Query items guide][dotnet-query-items] for more details about the query language.

</details>

Once you could confirm you are getting the expected response by calling the HTTP endpoint of the function, you can go ahead and update the configuration of the demo Web App with the function's endpoint.

You can do that by setting the value of the **Static Web App** environment variable `TRANSCRIPTION_FETCHING_URL` to the url of the `GetTranscriptions` function inside the `Configuration` section exactly like the Lab 1.

</details>

If everything is working correctly, you will be able to see the list of transcriptions like this:

![Web App](assets/static-web-app-transcriptions.png)

## Getting transcriptions in real-time (50 min)

The next step is to listen to new transcriptions and show them in real-time on the demo Web App as they get generated.

To achieve this, you will be relying on [Azure Web PubSub][azure-web-pub-sub] for sending and receiving notifications as transcriptions get added to Cosmos DB.

The flow will be the following:

- A new function `CosmosToWebPubSub` will be triggered whenever a new transcription is added to Cosmos DB.
- The function will create a new notification in an [Azure Web PubSub hub][azure-web-pubsub-hub]. New transcription will fill in the content of the notification.
- The demo Web App will be listening to new messages on the same Web PubSub hub and will show new transcriptions on real-time.

### Publishing new transcriptions using Web PubSub for real-time communication

The next step is to use the Web PubSub instance to publish new transcriptions as they get added to Cosmos DB.

<div class="task" data-title="Task">

> Create a Cosmos DB-triggered function which publishes new records to your Web PubSub instance:
>
> - Name: `CosmosToWebPubSub`
> - Trigger: `Cosmos DB`
> - Action: Detect new transcriptions and publish them to Azure Web PubSub in `JSON` format
> - Audience: All clients listening to updates on the Web PubSub hub

</div>

<div class="tip" data-title="Resources">

> [Cosmos DB input binding][cosmosdb-input-binding]<br> 
> [Web PubSub output binding][web-pubsub-output-binding]

</div>

<details>
<summary>📚 Toggle solution </summary>

Add a new Cosmos DB-triggered function `CosmosToWebPubSub` to your Function App and use the following settings:

| App setting                         | Description                           |
|-------------------------------------|---------------------------------------|
| WEB_PUBSUB_HUB_ID                   | Web PubSub resource name              |  
| WEB_PUBSUB_CONNECTION_STRING        | Web PubSub primary connection string  |

You need to update the App settings of the Function App by adding the 2 new settings:

- Set `WEB_PUBSUB_HUB_ID` with the name of the Web PubSub resource
- Set `WEB_PUBSUB_CONNECTION_STRING` with the primary connection string in the `Keys` section of your Web PubSub resource on Azure.

#### .NET 8 implementation

Let's create a Cosmos DB triggered function using the template `CosmosDBTrigger` and use the `WebPubSub` extension to send notifications to the `Web PubSub` hub using the `WebPubSubOutput` output binding.

```sh
# Create a skeleton of a Cosmos DB triggered function
func new --name CosmosToWebPubSub --template "CosmosDBTrigger"

# Use the latest version of the Web PubSub Nuget package (prerelease) to interact with Web PubSub
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.WebPubSub --version 1.7.0-beta.1
```

This should create a `CosmosToWebPubSub.cs` file with a function that will trigger whenever you add a new item to a Cosmos DB collection.

Next, you will need to update the `Run` method with the following contents:

```csharp
[Function(nameof(CosmosToWebPubSub))]
[WebPubSubOutput(Hub = "%WEB_PUBSUB_HUB_ID%", Connection = "WEB_PUBSUB_CONNECTION_STRING")]
public SendToAllAction? Run(
    [CosmosDBTrigger(
        databaseName: "%COSMOS_DB_DATABASE_NAME%",
        collectionName: "%COSMOS_DB_CONTAINER_ID%",
        ConnectionStringSetting = "COSMOS_DB_CONNECTION_STRING_SETTING",
        CreateLeaseCollectionIfNotExists = true,
        LeaseCollectionName = "leases")
    ] IReadOnlyList<Transcription> input
)
{
    if (input != null && input.Count > 0)
    {
        _logger.LogInformation("Document Id: " + input[0].id);

        return new SendToAllAction
        {
            Data = BinaryData.FromString(JsonSerializer.Serialize(input[0])),
            DataType = WebPubSubDataType.Json
        };
    }

    return null;
}
```

As the notification data will be sent in the JSON format, you will need to add the following dependency at the top of the file:

```csharp
using System.Text.Json;
```

Notice the use of the 2 bindings to simplify the interaction with other services:

- `CosmosDBTrigger`: this trigger will detect automatically new items added to the collection and run the function whenever that happens
- `WebPubSubOutput`: this output binding will send a notification to the hub defined in its constructor. To send a notification to everyone in the hub, you need to return a `SendToAllAction` instance.

</details>

### Consuming new transcriptions from Web PubSub

The last step is to consume the newly published transcriptions in the demo Web App from the Web PubSub hub.

<div class="task" data-title="Task">

> Set environment variables in your Static Web App for:
> - The connection string of the Web PubSub with `WPS_CONNECTION_STRING`
> - The Web PubSub resource name with `WPS_HUB_NAME`
> - Ensure that new transcriptions are displayed in the Web App as you upload new audio files.

</div>

<div class="tip" data-title="Resources">

> [Publish and consume messages from Web PubSub][publish-and-consume-from-web-pubsub]

</div>

<details>
<summary>📚 Toggle solution</summary>

You can retrieve a connection string for the Web PubSub directly with the Azure Portal or using this command line:

```sh
az webpubsub key show \
    --name <unique-web-pubsub-instance-name> \
    --resource-group <resource-group> \
    --query primaryConnectionString \
    --output tsv
```

</details>

## Lab 2 : Summary

By now you should have a solution that :

- Uploads audio files to a Storage Account
- Transcribes the uploaded audio file and displays it in a web interface in real-time
- Retrieves the latest 50 transcriptions using a RESTful API

The entire architecture is **Serverless** : Azure compute resources will only be billed when a new audio file is uploaded via the Azure Function API (after a large number of activations). You can leave the resources for a few days to see that _no compute resources are billed_ when no audio file is uploaded.

[cosmosdb-input-binding]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-input?tabs=python-v1%2Cin-process%2Cfunctionsv2&pivots=programming-language-python
[offset-limit-clause-in-cosmosdb]: https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/offset-limit
[cosmosdb-connection-string]: https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-dotnet-get-started?tabs=azure-cli%2Cwindows#retrieve-your-account-connection-string
[azure-web-pubsub]: https://learn.microsoft.com/en-us/azure/azure-web-pubsub/overview
[create-web-pubsub-instance]: https://learn.microsoft.com/en-us/azure/azure-web-pubsub/howto-develop-create-instance?tabs=CLI&pivots=method-azure-portal
[web-pubsub-output-binding]: https://learn.microsoft.com/en-us/azure/azure-web-pubsub/reference-functions-bindings?tabs=csharp#output-binding
[publish-and-consume-from-web-pubsub]: https://learn.microsoft.com/en-us/azure/azure-web-pubsub/tutorial-pub-sub-messages
[cosmosdb-input]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-input?tabs=python-v2%2Cisolated-process%2Cextensionv4&pivots=programming-language-csharp
[dotnet-query-items]: https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-dotnet-query-items
[azure-web-pub-sub]: https://learn.microsoft.com/en-us/azure/azure-web-pubsub/overview
[azure-web-pubsub-hub]: https://learn.microsoft.com/en-us/azure/azure-web-pubsub/key-concepts

---

# Closing the workshop

Once you're done with this lab you can delete the resource group you created at the beginning.

To do so, click on `delete resource group` in the Azure Portal to delete all the resources and audio content at once. The following Az-Cli command can also be used to delete the resource group :

```bash
# Delete the resource group with all the resources
az group delete --name <resource-group>
```
