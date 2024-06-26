resource "azurerm_static_web_app" "this" {
  name                = format("stapp-%s", local.resource_suffix_kebabcase)
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  tags                = local.tags
  sku_tier            = var.swa_tier
  sku_size            = var.swa_size

  app_settings = {
    "FILE_UPLOADING_FORMAT" = "binary"
  }
}
