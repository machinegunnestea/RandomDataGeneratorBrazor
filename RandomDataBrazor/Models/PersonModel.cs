namespace RandomDataBrazor.Models
{
    public record PersonModel
    {
        public int Number { get; set; }
        public int Identifier { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
}
