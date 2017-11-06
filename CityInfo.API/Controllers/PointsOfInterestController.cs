using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Interfaces;
using CityInfo.API.Model;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CityInfo.API.Controllers
{
    //this class return child resources(child entities)
    [Route("api/cities")]
    public class PointsOfInterestController : Controller
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, 
            IMailService mailService, ICityInfoRepository cityInfoRepository)
        {
            _logger = logger;
            _mailService = mailService;
            _cityInfoRepository = cityInfoRepository;
        }

        /// <summary>
        /// Get all the records from the API
        /// </summary>
        /// <param name="cityId"></param>
        /// <returns></returns>
        [HttpGet("{cityId}/pointsofinterest")]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            try
            {

                if (!_cityInfoRepository.CityExists(cityId))
                {
                    _logger.LogInformation($"City with id{cityId} wasn't found accessing points of interest.");
                    return NotFound();
                }

                var pointsOfInterestForCity = _cityInfoRepository.GetPointOfInterestForCity(cityId);

                var pointsOfInterestForCityResults =
                    Mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity);

                return Ok(pointsOfInterestForCityResults);
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"Exception while getting points of interest for city with id {cityId}.", ex);
                return StatusCode(500, "A problem happened while handling your request");
            }
        }
        /// <summary>
        /// Get a record with an id from API
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{cityId}/pointsofinterest/{id}", Name = "GetPointOfInterest")]
        public IActionResult GetPointOfInterest(int cityId, int id)
        {

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (pointOfInterest == null)
            {
                return NotFound();
            }

            var pointOfInterestResult = Mapper.Map<PointOfInterestDto>(pointOfInterest);

            return Ok(pointOfInterestResult);
        }

        /// <summary>
        /// Creating/Adding Action to API
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="pointOfInterest"></param>
        /// <returns></returns>
        [HttpPost("{cityId}/pointsofinterest")]
        public IActionResult CreatePointOfInterest(int cityId,
            [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            //check for nulls on the params
            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            //do a check if description matches with name and throw exception
            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "Theprovided description should be different from the name");
            }

            //check if the modelstate is valid
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var finalPointsOfInterest = Mapper.Map<PointOfInterest>(pointOfInterest);

            //Adding points of interest to city
            _cityInfoRepository.AddPointOfInterestForCity(cityId, finalPointsOfInterest);

            //doing a check to persist in DB 
            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling you request.");
            }

            var createdPointOfInterestToReturn = Mapper.Map<PointOfInterest>(finalPointsOfInterest);

            return CreatedAtRoute("GetPointOfInterest", new {cityId = cityId, id = createdPointOfInterestToReturn.Id}, createdPointOfInterestToReturn);
        }

        /// <summary>
        /// Updating Action from API
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="id"></param>
        /// <param name="pointOfInterest"></param>
        /// <returns></returns>
        [HttpPut("{cityId}/pointsofinterest/{id}")]
        public IActionResult UpdatePointOfInterest(int cityId, int id,
            [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            //do a check if description matches with name and throw exception
            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "Theprovided description should be different from the name");
            }

            //check if the modelstate is valid
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            Mapper.Map(pointOfInterest, pointOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling you request.");
            }

            //default convention for returning updating values back to API 
            return NoContent();
        }

        /// <summary>
        /// Updating Certain resource from the API using JSON Patch Document
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("{cityId}/pointsofinterest/{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id,
            [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = Mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

            patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (pointOfInterestToPatch.Description == pointOfInterestToPatch.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            TryValidateModel(pointOfInterestToPatch);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            return NoContent();
        }


        /// <summary>
        /// Deleting a resource from the API
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{cityId}/pointsofinterest/{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

           _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling you request.");
            }

            //custom service
            _mailService.Send("Point of interest deleted.", $"Point of interest {pointOfInterestEntity.Name} with id" +
                                                            $"{pointOfInterestEntity.Id} was deleted");
            return NoContent();
        }
    }
}
