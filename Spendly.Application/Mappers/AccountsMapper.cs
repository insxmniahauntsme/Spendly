using Riok.Mapperly.Abstractions;
using Spendly.Data.Entities;
using Spendly.Domain.Models;

namespace Spendly.Application.Mappers;

[Mapper]
public static partial class AccountsMapper
{
    public static partial AccountEntity ToEntity(this Account account);
    
    public static partial Account ToModel(this AccountEntity account);
}