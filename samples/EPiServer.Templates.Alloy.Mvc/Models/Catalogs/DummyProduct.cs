using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;

namespace EPiServer.Templates.Alloy.Models.Catalogs
{
    [CatalogContentType(
            DisplayName = "DummyProduct", 
            GUID = "4965d65c-9415-43dd-ad9c-3d4d080fd27d", 
            Description = "")]
    public class DummyProduct : ProductContent
    {

    }
}
