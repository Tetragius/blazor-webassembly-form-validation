using System;
using System.Threading.Tasks;
using BlazorApp.Api.Services;
using BlazorApp.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BlazorApp.Api.Controllers
{
    [ApiController]
    [Route("api/card")]
    public class CardController : ControllerBase
    {
        private readonly MemoryCacheRepository _cacheRepository;
        private readonly string _itemCacheKey = "my_card_1";

        public CardController(IMemoryCache memCache)
        {
            _cacheRepository = new MemoryCacheRepository(memCache);
        }

        [AllowAnonymous]
        [HttpGet("ping")]
        public ActionResult<ResultDto> Ping() => new ResultDto {Ok = true, Message = "pong"};

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<CardModel>> Get()
        {
            var card = await _cacheRepository.GetFactoryMethodAsync<CardModel>(_itemCacheKey);
            if (card == null)
            {
                // setup first loaded card
                card = new CardModel();
                // how long place in cache
                await _cacheRepository.SetFactoryMethodAsync(card, _itemCacheKey, DateTimeOffset.UtcNow.AddMinutes(5));
            }

            return Ok(card);
        }

        [AllowAnonymous]
        [HttpPut]
        public async Task<ActionResult<ResultDto>> Update(CardModel request)
        {
            var card = await _cacheRepository.GetFactoryMethodAsync<CardModel>(_itemCacheKey);
            if (card == null)
                return NotFound();
            card.Email = request.Email;
            card.Password = request.Password;
            card.Title = request.Title;
            return Ok(new ResultDto
            {
                Ok = true,
                Message = "updated"
            });
        }
    }
}