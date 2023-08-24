using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class NearCinemaFilterDTO
    {
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        private int distanceInKm = 5;
        private int maxDistanceInKm = 50;
        public int DistanceInKm
        {
            get
            {
                return distanceInKm;
            }
            set
            {
                distanceInKm = (value > maxDistanceInKm) ? maxDistanceInKm : value;
            }
        }
    }
}
