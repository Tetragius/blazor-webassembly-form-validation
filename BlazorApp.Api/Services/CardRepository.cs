using System;
using System.Threading.Tasks;
using BlazorApp.Model.Dto;
using Microsoft.Extensions.Caching.Memory;

namespace BlazorApp.Api.Services
{
    public class CardRepository : ICardRepository
    {
        private readonly MemoryCacheRepository _cacheRepository;
        private readonly string _itemCacheKey = "my_card_1";

        public CardRepository(IMemoryCache memCache)
        {
            _cacheRepository = new MemoryCacheRepository(memCache);
        }

        public async Task<CardModel> GetAsync()
        {
            return await _cacheRepository.GetFactoryMethodAsync<CardModel>(_itemCacheKey);
        }

        public async Task<ResultDto> SetAsync(CardModel model)
        {
            await _cacheRepository.SetFactoryMethodAsync(_itemCacheKey, model, DateTimeOffset.UtcNow.AddDays(1));
            return new ResultDto {Ok = true};
        }
    }

    public interface ICardRepository
    {
        Task<CardModel> GetAsync();
        Task<ResultDto> SetAsync(CardModel model);
    }
}