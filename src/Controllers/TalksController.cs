using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CoreCodeCamp.Data;
using CoreCodeCamp.Model;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/talks")]
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;

        public TalksController(ICampRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // GET: api/camps/{moniker}/talks")
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TalkModel>>> GetTalks(string moniker)
        {
            try
            {
                var results = await _repository.GetTalksByMonikerAsync(moniker, true);
                return _mapper.Map<TalkModel[]>(results);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
        }
        
        // GET: api/camps/{moniker}Talks/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> GetTalk(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, true);

                if (talk == null) return NotFound();
                
                return _mapper.Map<TalkModel>(talk);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
        }

        // PUT: api/camps/{moniker}Talks/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> PutTalk(string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, true);

                if (talk == null) return NotFound("Couldn't find the talk");

                if (talk.TalkId != model.TalkId) return BadRequest("Cannot update non-matching talk");
                
                _mapper.Map(model, talk);

                if (model.Speaker != null)
                {
                    var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null) talk.Speaker = speaker;
                }
                
                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<TalkModel>(talk);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
            return BadRequest("Could not update talk");
        }

        // POST: api/camps/{moniker}/Talks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TalkModel>> PostTalk(String moniker, TalkModel model)
        {
            try
            {
                var camp = await _repository.GetCampAsync(moniker, true);
                if (camp == null) return BadRequest("Camp does not exist");
                
                var talk = _mapper.Map<Talk>(model);
                talk.Camp = camp;

                if (model.Speaker == null) return BadRequest("Speaker ID is required");
                var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found");

                talk.Speaker = speaker;
                
                _repository.Add(talk);

                if (await _repository.SaveChangesAsync())
                {
                    return CreatedAtAction("GetTalk",
                        new { moniker, id = talk.TalkId },
                        _mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
            return BadRequest("Failed to save new Talk");
        }
        
        // DELETE: api/camps/{moniker/}Talks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTalk(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, true);

                if (talk == null) return NotFound();
                
                _repository.Delete(talk);

                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
        
            return BadRequest("Failed to delete talk");
        }
        
    }
}
