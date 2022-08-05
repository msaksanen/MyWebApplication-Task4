namespace MyWebApplication
{
    internal record Person
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";

        public string Password { get; set; } = "";
        public string Created { get; set; } = DateTime.Now.ToShortDateString();
        public string LastLogin { get; set; } = "";
        public bool IsOnline { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
    }
}
