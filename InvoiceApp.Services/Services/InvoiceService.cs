
using InvoiceApp.Data.Models.IRepository;
using InvoiceApp.Data.Models;
using InvoiceApp.Services.IServices;
using Microsoft.Extensions.Logging;
using InvoiceApp.Data.DTO;
using Microsoft.AspNetCore.Http;
using InvoiceApp.SD;
using InvoiceApp.Data.Enums;
using AutoMapper;
using InvoiceApp.Services.Helper;

namespace InvoiceApp.Services.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ILogger<InvoiceService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IInvoiceIdService _invoiceIdService;

        public InvoiceService(ILogger<InvoiceService> logger, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IMapper mapper, IInvoiceIdService invoiceIdService)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _invoiceIdService = invoiceIdService;
        }
        public async Task<ResponseDto<string>> AddInvoiceAsync(InvoiceRequestDto invoiceRequestDto)
        {
            var userEmail = _httpContextAccessor.HttpContext.Items["Email"]; 
            var userId = _httpContextAccessor.HttpContext.Items["UserId"]; 
            _logger.LogInformation("Attempting to Create New Invoice for {User} at {time}", userEmail, Contants.currDateTime);

            var frontendId = await _invoiceIdService.GenerateUniqueInvoiceIdForUserAsync((string)userId);
            var response = new ResponseDto<string>();

            //if (Enum.TryParse(invoiceRequestDto.Status, ignoreCase: true, out InvoiceStatus statusEnum) && statusEnum == InvoiceStatus.Paid)
            //{
            //    _logger.LogWarning("Invoice status is {0}. There is a problem with the status", invoiceRequestDto.Status);
            //    response.IsSuccess = false;
            //    response.Result = ErrorMessages.DefaultError;
            //    response.Message = "The invoice status can either be Draft or Pending";
            //    return response;
            //}

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = _mapper.Map<Invoice>(invoiceRequestDto);
                invoice.UserID = userId as string;
                invoice.Status = invoiceRequestDto.isReady ? InvoiceStatus.Pending : InvoiceStatus.Draft;
                invoice.CreatedAt = DateTime.Now;
                invoice.Created_at = DateTime.Now;
                invoice.FrontendId = frontendId;

                await _unitOfWork.InvoiceRepository.AddAsync(invoice);

                await _unitOfWork.SaveAsync(CancellationToken.None);
                await _unitOfWork.CommitAsync();

                // if isReady you need to send this the email provided 
                _logger.LogInformation("Invoice added successfully");

                response.IsSuccess = true;
                response.Message = "Invoice succesfully created";
                response.Result = invoice.FrontendId;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error creating invoice");
                response.Message = $"An error occurred: {ex.Message}";
                response.Result = ErrorMessages.DefaultError;
                response.IsSuccess = false;
            }

            return response;
        }

        public async Task<ResponseDto<InvoiceResponseDto>> GetInvoiceById(string invoiceId)
        {
            var userEmail = _httpContextAccessor.HttpContext.Items["Email"];
            var userId = _httpContextAccessor.HttpContext.Items["UserId"];
            _logger.LogInformation("Attempting to retrieve an invoice at {time}", Contants.currDateTime);

            var response = new ResponseDto<InvoiceResponseDto>();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // check if the invoice exist 
                var result = await _unitOfWork.InvoiceRepository.GetAllIncludingAsync(
                                invoice => invoice.User,
                                invoice => invoice.SenderAddress,
                                invoice => invoice.ClientAddress,
                                invoice => invoice.Items
                            );
                var invoice = result.FirstOrDefault(invoice => invoice.Id == invoiceId);

                if (invoice == null)
                {
                    _logger.LogWarning("The invoice Id {0} doesn't exist", invoiceId);
                    response.IsSuccess = false;
                    response.Message = "The invoice doesn't exist";
                    return response;
                }

                // check if the user is the one that created the invoice 
                if (userId as string != invoice.UserID)
                {
                    _logger.LogWarning("The invoice {0} doesn't exist for this user {1}", invoiceId, userEmail);
                    response.IsSuccess = false;
                    response.Message = "The invoice doesn't exist";
                    return response;
                }

                // if the user created the user, return the invoice 
                var invoiceToReturn = _mapper.Map<InvoiceResponseDto>(invoice);
                _logger.LogInformation("Invoice exist in the database!!!");

                response.IsSuccess = true;
                response.Message = "Invoice succesfully returned";
                response.Result = invoiceToReturn;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error getting invoice");
                response.Message = $"An error occurred: {ex.Message}";
                response.IsSuccess = false;
            }

            return response;
        }

        public async Task<ResponseDto<List<InvoiceResponseDto>>> GetAllInvoicesForUserAsync(string userId)
        {
            var userEmail = _httpContextAccessor.HttpContext.Items["Email"];
            userId ??= (string)_httpContextAccessor.HttpContext.Items["UserId"];
            _logger.LogInformation("Attempting to retrieve all invoices for user at {time}", Contants.currDateTime);

            var response = new ResponseDto<List<InvoiceResponseDto>>();
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Query all invoices that belong to the user
                var result = await _unitOfWork.InvoiceRepository.GetAllIncludingAsync(
                                invoice => invoice.User,
                                invoice => invoice.SenderAddress,
                                invoice => invoice.ClientAddress,
                                invoice => invoice.Items
                            );
                var invoices = result.Where(invoice => invoice.UserID == userId);

                if (!invoices.Any())
                {
                    _logger.LogWarning("No invoices found for the user {0}", userEmail);
                    response.IsSuccess = false;
                    response.Message = "No invoices found";
                    return response;
                }

                // Map the result to DTOs
                var invoicesToReturn = _mapper.Map<List<InvoiceResponseDto>>(invoices);
                await _unitOfWork.CommitAsync();

                response.IsSuccess = true;
                response.Message = "Invoices successfully returned";
                response.Result = invoicesToReturn;
                _logger.LogInformation("Invoices retrieved for the user {0}", userEmail);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error getting invoices for the user");
                response.Message = $"An error occurred: {ex.Message}";
                response.IsSuccess = false;
            }

            return response;
        }

        public async Task<ResponseDto<PaginatedList<InvoiceResponseDto>>> GetAllInvoicesForUserAsync(string userId, PaginationParameters paginationParameters)
        {
            _logger.LogInformation("Attempting to retrieve paginated invoices for user {UserId} at {Time}", userId, DateTime.UtcNow);

            try
            {
                var userEmail = _httpContextAccessor.HttpContext.Items["Email"] as string;
                userId ??= _httpContextAccessor.HttpContext.Items["UserId"] as string;

                // Ensure we have a valid user ID
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No user ID provided for retrieving invoices.");
                    return new ResponseDto<PaginatedList<InvoiceResponseDto>>
                    {
                        IsSuccess = false,
                        Message = "User ID is required."
                    };
                }

                var result = await _unitOfWork.InvoiceRepository.GetAllIncludingAsync(
                        invoice => invoice.User,
                        invoice => invoice.SenderAddress,
                        invoice => invoice.ClientAddress,
                        invoice => invoice.Items);

                var invoicesQuery = result.Where(invoice => invoice.UserID == userId).AsQueryable();

                var paginatedInvoices = await PaginatedList<Invoice>.CreateAsync(invoicesQuery, paginationParameters.PageNumber, paginationParameters.PageSize);
                var invoiceDtos = _mapper.Map<List<InvoiceResponseDto>>(paginatedInvoices);

                var response = new PaginatedList<InvoiceResponseDto>(invoiceDtos, paginatedInvoices.Count, paginationParameters.PageNumber, paginationParameters.PageSize);

                return new ResponseDto<PaginatedList<InvoiceResponseDto>>
                {
                    IsSuccess = true,
                    Message = "Invoices retrieved successfully.",
                    Result = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving invoices for user {UserId}", userId);
                return new ResponseDto<PaginatedList<InvoiceResponseDto>>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDto<bool>> EditInvoiceAsync(string invoiceId, InvoiceRequestDto invoiceRequestDto)
        {
            _logger.LogInformation("Attempting to edit invoice {InvoiceId}", invoiceId);

            try
            {
                var invoice = await _unitOfWork.InvoiceRepository.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found.", invoiceId);
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Invoice not found." };
                }

                if (invoice.Status == InvoiceStatus.Paid)
                {
                    _logger.LogWarning("Attempted to edit paid invoice {InvoiceId}.", invoiceId);
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Paid invoices cannot be edited." };
                }

                var invoiceFromRequest = new Invoice();
                // Map changes
                _mapper.Map(invoiceRequestDto, invoiceFromRequest);
                invoiceFromRequest.Status = invoiceRequestDto.isReady ? InvoiceStatus.Pending : InvoiceStatus.Draft;
                List<Item> oldItem = invoice.Items;
                invoice.Items = invoiceFromRequest.Items;

                // Do the address and item checking 
                var clientAddressIsTheSame = CheckAddress(invoiceFromRequest.ClientAddress, invoice.ClientAddress);
                var sendersAddressIsTheSame = CheckAddress(invoiceFromRequest.SenderAddress, invoice.SenderAddress);
                
                Address oldClientAddress = invoice.ClientAddress;
                Address oldSenderAddress = invoice.SenderAddress;
                
                if (!clientAddressIsTheSame)
                {
                    invoice.ClientAddress = invoiceFromRequest.ClientAddress;
                }

                if (!sendersAddressIsTheSame)
                {
                    invoice.SenderAddress = invoiceFromRequest.SenderAddress;
                }

                // delete the ItemObject from the database 
                await _unitOfWork.ItemRepository.DeleteRangeAsync(oldItem);
                await _unitOfWork.InvoiceRepository.UpdateAsync(invoice);

                // if addressIsFalse
                if (!clientAddressIsTheSame)
                {
                    // delete the address 
                    await _unitOfWork.AddressRepository.DeleteAsync(oldClientAddress);
                }

                if (!sendersAddressIsTheSame)
                {
                    // delete the address 
                    await _unitOfWork.AddressRepository.DeleteAsync(oldSenderAddress);
                }

                await _unitOfWork.SaveAsync(CancellationToken.None);

                _logger.LogInformation("Invoice {InvoiceId} updated successfully.", invoice.Id);

                return new ResponseDto<bool> { IsSuccess = true, Message = "Invoice updated successfully.", Result = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing invoice {InvoiceId}.", invoiceId);
                return new ResponseDto<bool> { IsSuccess = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<ResponseDto<bool>> DeleteInvoiceAsync(string invoiceId)
        {
            var userId = _httpContextAccessor.HttpContext.Items["UserId"] as string;
            _logger.LogInformation("Attempting to delete invoice {InvoiceId} for user {UserId} at {Time}", invoiceId, userId, DateTime.UtcNow);

            try
            {
                var invoice = await _unitOfWork.InvoiceRepository.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found", invoiceId);
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Invoice not found" };
                }

                if (invoice.UserID != userId)
                {
                    _logger.LogWarning("User {UserId} does not have permission to delete invoice {InvoiceId}", userId, invoiceId);
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Unauthorized to delete this invoice" };
                }

                await _unitOfWork.InvoiceRepository.DeleteAsync(invoice);
                await _unitOfWork.AddressRepository.DeleteAsync(invoice.ClientAddress);
                await _unitOfWork.AddressRepository.DeleteAsync(invoice.SenderAddress);
                await _unitOfWork.SaveAsync(CancellationToken.None);
                _logger.LogInformation("Invoice {InvoiceId} deleted successfully", invoiceId);

                return new ResponseDto<bool> { IsSuccess = true, Message = "Invoice deleted successfully", Result = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice {InvoiceId}", invoiceId);
                return new ResponseDto<bool> { IsSuccess = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<ResponseDto<bool>> MarkInvoiceAsPaidAsync(string invoiceId)
        {
            _logger.LogInformation("Marking invoice {InvoiceId} as paid at {Time}", invoiceId, DateTime.UtcNow);
            var response = new ResponseDto<bool> { IsSuccess = false };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await _unitOfWork.InvoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                {
                    _logger.LogWarning("Invoice {InvoiceId} not found.", invoiceId);
                    response.Message = "Invoice not found.";
                    return response;
                }

                if (invoice.Status == InvoiceStatus.Paid)
                {
                    _logger.LogInformation("Invoice {InvoiceId} is already marked as paid.", invoiceId);
                    response.IsSuccess = true;
                    response.Message = "Invoice is already marked as paid.";
                    response.Result = true;
                    return response;
                }

                invoice.Status = InvoiceStatus.Paid;
                //invoice.PaymentDate = DateTime.UtcNow; // Assuming there's a PaymentDate field
                await _unitOfWork.InvoiceRepository.UpdateAsync(invoice);
                await _unitOfWork.SaveAsync(CancellationToken.None);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Invoice {InvoiceId} marked as paid successfully.", invoiceId);
                response.IsSuccess = true;
                response.Message = "Invoice marked as paid successfully.";
                response.Result = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error marking invoice {InvoiceId} as paid.", invoiceId);
                response.Message = $"An error occurred: {ex.Message}";
            }

            return response;
        }
           
        private static bool CheckAddress(Address newAddress, Address currAddress)
        {
            return newAddress.Street == currAddress.Street &&
                   newAddress.City == currAddress.City &&
                   newAddress.PostCode == currAddress.PostCode &&
                   newAddress.Country == currAddress.Country;
        }
    }
}
