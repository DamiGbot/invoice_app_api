
using InvoiceApp.Data.Models;
using InvoiceApp.Data.Models.IRepository;
using InvoiceApp.Services.IServices;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace InvoiceApp.Services.Services
{
    public class InvoiceIdService : IInvoiceIdService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InvoiceIdService> _logger;
        private readonly ConcurrentDictionary<string, HashSet<string>> _userSpecificIdCache = new ConcurrentDictionary<string, HashSet<string>>();

        public InvoiceIdService(IUnitOfWork unitOfWork, ILogger<InvoiceIdService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<string> GenerateUniqueInvoiceIdForUserAsync(string userId)
        {
            var userSpecificIds = _userSpecificIdCache.GetOrAdd(userId, new HashSet<string>());
            string newId;
            var random = new Random();

            do
            {
                newId = $"{(char)('A' + random.Next(0, 26))}{(char)('A' + random.Next(0, 26))}{random.Next(1000, 9999)}";
            } while (userSpecificIds.Contains(newId));

            userSpecificIds.Add(newId);
            var tracker = new InvoiceIdTracker { UserId = userId, FrontendId = newId };
            await _unitOfWork.invoiceIdTrackersRepository.AddAsync(tracker);
            await _unitOfWork.SaveAsync(CancellationToken.None);

            return newId;
        }

        public async Task RefreshCacheAsync()
        {
            var invoiceIdTrackers = await _unitOfWork.invoiceIdTrackersRepository.GetAllAsync();
            _userSpecificIdCache.Clear();

            foreach (var tracker in invoiceIdTrackers)
            {
                var userSpecificIds = _userSpecificIdCache.GetOrAdd(tracker.UserId, new HashSet<string>());
                userSpecificIds.Add(tracker.FrontendId);
            }

            _logger.LogInformation("Invoice ID cache refreshed.");
        }
    }
}
