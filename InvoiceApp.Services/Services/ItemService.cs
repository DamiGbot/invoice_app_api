
using InvoiceApp.Data.Models.IRepository;
using InvoiceApp.Services.IServices;
using Microsoft.Extensions.Logging;

namespace InvoiceApp.Services.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly ILogger<ItemService> _logger;

        public ItemService(IItemRepository itemRepository, ILogger<ItemService> logger)
        {
            _itemRepository = itemRepository;
            _logger = logger;
        }

        //public async Task AddItemAsync(ItemDto itemDto)
        //{
        //    _logger.LogInformation("Adding new item to invoice {@CreateItemDto}", createItemDto);
        //    try
        //    {
        //        var item = new Item
        //        {
        //            Name = itemDto.Name,
        //            Quantity = itemDto.Quantity,
        //            Price = itemDto.Price,
        //            Total = itemDto.Total,
        //            InvoiceID = itemDto.InvoiceID
        //        };

        //        await _itemRepository.AddAsync(item);
        //        _logger.LogInformation("Item added successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error adding item");
        //        throw;
        //    }
        //}

        //public async Task DeleteItemAsync(int itemId)
        //{
        //    try
        //    {
        //        var item = await _itemRepository.GetByIdAsync(itemId);
        //        if (item != null)
        //        {
        //            await _itemRepository.DeleteAsync(item);
        //            _logger.LogInformation("Item deleted successfully");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error deleting item");
        //        throw;
        //    }
        //}

    }
}
