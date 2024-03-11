using AutoMapper;
using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Enums;
using InvoiceApp.Data.Models;

namespace InvoiceAppApi.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AddressDto, Address>();

            CreateMap<ItemDto, Item>();

            CreateMap<InvoiceRequestDto, Invoice>()
                .ForMember(dest => dest.PaymentDue, opt => opt.MapFrom(src => DateTime.Now.AddDays(src.PaymentTerms)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse(typeof(InvoiceStatus), src.Status, true)))
                .ForMember(dest => dest.SenderAddress, opt => opt.MapFrom(src => src.SenderAddress))
                .ForMember(dest => dest.ClientAddress, opt => opt.MapFrom(src => src.ClientAddress))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Items.Sum(item => item.Total)))
                .AfterMap((src, dest) => {
                    if (Enum.TryParse(src.Status, ignoreCase: true, out InvoiceStatus statusEnum))
                    {
                        switch (statusEnum)
                        {
                            case InvoiceStatus.Paid:
                                dest.CreatedAt = DateTime.Now.AddDays(-src.PaymentTerms);
                                break;
                            default:
                                dest.CreatedAt = DateTime.Now;
                                break;
                        }
                    } 
                });

            CreateMap<Invoice, InvoiceResponseDto>();

            CreateMap<Item, ItemDto>();
        }
    }
}
