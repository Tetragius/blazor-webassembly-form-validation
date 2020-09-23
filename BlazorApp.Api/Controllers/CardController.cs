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
        private readonly ICardRepository _repository;
        
        public CardController(ICardRepository repo)
        {
            _repository = repo;
        }

        [AllowAnonymous]
        [HttpGet("ping")]
        public ActionResult<ResultDto> Ping() => new ResultDto {Ok = true, Message = "pong"};

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<CardModel>> Get()
        {
            var card = await _repository.GetAsync();
            if (card == null)
            {
                // setup first loaded card
                card = new CardModel();
                await _repository.SetAsync(card);
            }

            return Ok(card);
        }

        [AllowAnonymous]
        [HttpPut]
        public async Task<ActionResult<ResultDto>> Update(CardModel request)
        {
            var card = await _repository.GetAsync();
            if (card == null)
                return NotFound();
            
            card.Email = request.Email;
            card.Password = request.Password;
            card.Title = request.Title;

            await _repository.SetAsync(card);
            
            return Ok(new ResultDto
            {
                Ok = true,
                Message = "updated"
            });
        }
    }
}