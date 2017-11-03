using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CityInfo.API.Entities;
using CityInfo.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Repository
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context = context;
        }

        public IEnumerable<City> GetCities()
        {
            return _context.Cities.OrderBy(c => c.Name).ToList();
        }

        public City GetCity(int cityId, bool includePointOfInterest)
        {
            if (includePointOfInterest)
            {
                return _context.Cities.Include(c => c.PointsOfInterest)
                    .FirstOrDefault(c => c.Id == cityId);
            }

            return _context.Cities.FirstOrDefault(c => c.Id == cityId);
        }

        public IEnumerable<PointOfInterest> GetPointOfInterestForCity(int cityId)
        {
            return _context.PointOfInterest.Where(p => p.CityId == cityId).ToList();
        }

        public PointOfInterest GetPointOfInterestForCity(int cityId, int pointOfInterestId)
        {
            return _context.PointOfInterest.FirstOrDefault(p => p.CityId == cityId && p.Id == pointOfInterestId);
        }
    }
}
