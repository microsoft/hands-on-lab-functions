resource "azurerm_cognitive_account" "this" {
  name                          = format("oai-%s", local.resource_suffix_kebabcase)
  location                      = azurerm_resource_group.this.location
  resource_group_name           = azurerm_resource_group.this.name
  kind                          = "OpenAI"
  sku_name                      = "S0"
  public_network_access_enabled = false
  custom_subdomain_name         = format("azure-open-ai-%s", local.resource_suffix_kebabcase)
  tags                          = local.tags
}

resource "azurerm_cognitive_deployment" "speech_to_text_model" {
  name                 = local.speech_to_text_model
  cognitive_account_id = azurerm_cognitive_account.this.id
  model {
    format  = "OpenAI"
    name    = local.speech_to_text_model
    version = "001"
  }

  scale {
    type     = "Standard"
    capacity = 3
  }
}

