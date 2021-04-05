using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CoreCodeCamp.Data;
using CoreCodeCamp.Model;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps")]
    [ApiController]
    [ApiVersion("2.0")]
    public class Camps2Controller : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;

        public Camps2Controller(ICampRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // GET: api/Camps
        [HttpGet]
        // async Task<ActionResult<IEnumerable<Camp>>>
        public async Task<IActionResult> GetCamps(bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);
                var result = new
                {
                    Count = results.Length,
                    Results = _mapper.Map<CampModel[]>(results)
                };
                return Ok(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
            
        }

        // GET: api/Camps/SD2018
        [HttpGet("{moniker}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<CampModel>> GetCamp(string moniker)
        {
            try
            {
                var camp = await _repository.GetCampAsync(moniker);
                
                if (camp == null) return NotFound();
                
                return _mapper.Map<CampModel>(camp);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
        }
        
        // GET: api/Camps/SD2018
        [HttpGet("{moniker}")]
        [MapToApiVersion("1.1")]
        public async Task<ActionResult<CampModel>> GetCamp11(string moniker)
        {
            try
            {
                var camp = await _repository.GetCampAsync(moniker, true);
                
                if (camp == null) return NotFound();
                
                return _mapper.Map<CampModel>(camp);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
        }
        
        // GET: api/Camps
        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);
                
                if (!results.Any()) return NotFound();
                
                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
            
        }
        
        // PUT: api/Camps/SD2018
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> PutCamp(String moniker, CampModel model)
        {
            if (moniker != model.Moniker)
            {
                return BadRequest();
            }
            
            try
            {
                var oldCamp = await _repository.GetCampAsync(model.Moniker);
                if (oldCamp == null) return NotFound($"Could not find camp with moniker of {moniker}");

                _mapper.Map(model, oldCamp);

                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
        
            return NoContent();
        }
        //
        // POST: api/Camps
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CampModel>> PostCamp(CampModel model)
        {
            try
            {
                var exists = await _repository.GetCampAsync(model.Moniker);
                if (exists != null) return BadRequest("Moniker in use");
                
                var camp = _mapper.Map<Camp>(model);
                _repository.Add(camp);
                
                if (await _repository.SaveChangesAsync())
                {
                    return CreatedAtAction("GetCamp", new { moniker = camp.Moniker }, _mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "It's not you, it's us");
            }
            return BadRequest();
        }
        
        // DELETE: api/Camps/SD2018
        [HttpDelete("{moniker}")]
        public async Task<IActionResult> DeleteCamp(string moniker)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound();
                
                _repository.Delete(oldCamp);
                
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
            
            return BadRequest("Failed to delete camp");
        }
        
    }
}
