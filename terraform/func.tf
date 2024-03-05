resource "azurerm_linux_function_app" "standard" {
  name                = format("func-std-%s", local.resource_suffix_kebabcase)
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  storage_account_name       = azurerm_storage_account.func.name
  storage_account_access_key = azurerm_storage_account.func.primary_access_key
  service_plan_id            = azurerm_service_plan.this.id

  tags = local.tags

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    STORAGE_ACCOUNT_CONTAINER             = "audios"
    STORAGE_ACCOUNT_CONNECTION_STRING     = azurerm_storage_account.this.primary_connection_string
    APPINSIGHTS_INSTRUMENTATIONKEY        = azurerm_application_insights.this.instrumentation_key
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.this.connection_string
    COSMOS_DB_DATABASE_NAME               = local.cosmos_db_database_name
    COSMOS_DB_CONTAINER_ID                = local.cosmos_db_container_name
    COSMOS_DB_CONNECTION_STRING_SETTING   = azurerm_cosmosdb_account.this.primary_sql_connection_string
    WEB_PUBSUB_HUB_ID                     = azurerm_web_pubsub.this.name
    WEB_PUBSUB_CONNECTION_STRING          = azurerm_web_pubsub.this.primary_connection_string
  }

  site_config {
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }
}

resource "azurerm_linux_function_app" "durable" {
  name                = format("func-drbl-%s", local.resource_suffix_kebabcase)
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  storage_account_name       = azurerm_storage_account.func.name
  storage_account_access_key = azurerm_storage_account.func.primary_access_key
  service_plan_id            = azurerm_service_plan.this.id

  tags = local.tags

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    STORAGE_ACCOUNT_CONTAINER             = "audios"
    STORAGE_ACCOUNT_CONNECTION_STRING     = azurerm_storage_account.this.primary_connection_string
    APPINSIGHTS_INSTRUMENTATIONKEY        = azurerm_application_insights.this.instrumentation_key
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.this.connection_string
    COSMOS_DB_DATABASE_NAME               = local.cosmos_db_database_name
    COSMOS_DB_CONTAINER_ID                = local.cosmos_db_container_name
    COSMOS_DB_CONNECTION_STRING_SETTING   = azurerm_cosmosdb_account.this.primary_sql_connection_string
  }

  site_config {

    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }
}


resource "azurerm_linux_function_app" "whisper" {
  name                = format("func-whpr-%s", local.resource_suffix_kebabcase)
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  storage_account_name       = azurerm_storage_account.func.name
  storage_account_access_key = azurerm_storage_account.func.primary_access_key
  service_plan_id            = azurerm_service_plan.this.id

  tags = local.tags

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    STORAGE_ACCOUNT_CONTAINER             = "audios"
    STORAGE_ACCOUNT_CONNECTION_STRING     = azurerm_storage_account.this.primary_connection_string
    APPINSIGHTS_INSTRUMENTATIONKEY        = azurerm_application_insights.this.instrumentation_key
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.this.connection_string
    COSMOS_DB_DATABASE_NAME               = local.cosmos_db_database_name
    COSMOS_DB_CONTAINER_ID                = local.cosmos_db_container_name
    COSMOS_DB_CONNECTION_STRING_SETTING   = azurerm_cosmosdb_account.this.primary_sql_connection_string
    AZURE_OPEN_AI_DEPLOYMENT_ENDPOINT     = azurerm_cognitive_account.this.endpoint
    SPEECH_TO_TEXT_MODEL_DEPLOYMENT_NAME  = local.speech_to_text_model
  }

  site_config {
    application_stack {
      python_version = "3.9"
    }
  }
}
