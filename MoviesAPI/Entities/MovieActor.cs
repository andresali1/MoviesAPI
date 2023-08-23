namespace MoviesAPI.Entities
{
    public class MovieActor
    {
        public int ActorId { get; set; }
        public int MovieId { get; set; }
        public string Character { get; set; }
        public int Order_Num { get; set; }
        public Actor Actor { get; set; }
        public Movie Movie{ get; set; }
    }
}
