using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.API.Interfaces;
using CityInfo.API.Model;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/[controller]")]
    public class CitiesController : Controller
    {
        private readonly ICityInfoRepository _cityInfoRepository;

        public CitiesController(ICityInfoRepository cityInfoRepository)
        {
            _cityInfoRepository = cityInfoRepository;
        }


        [HttpGet()]
        public IActionResult GetCities()
        {
            var cityEntities = _cityInfoRepository.GetCities();
            var result = Mapper.Map<IEnumerable<CityWithoutPointsOfIntrestDto>>(cityEntities);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetCity(int id, bool includePointsOfInterest = false)
        {
            var city = _cityInfoRepository.GetCity(id, includePointsOfInterest);

            if (city == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                var cityResult = Mapper.Map<CityDto>(city);

                return Ok(cityResult);
            }

            var cityWithOutPointsOfInterestResult = Mapper.Map<CityWithoutPointsOfIntrestDto>(city);

            return Ok(cityWithOutPointsOfInterestResult);
        }
    }
}
