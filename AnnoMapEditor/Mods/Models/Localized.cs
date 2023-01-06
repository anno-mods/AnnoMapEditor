namespace AnnoMapEditor.Mods.Models
{
    public class Localized
    {
        public string? Chinese { get; set; }

        public string? English { get; set; }

        public string? French { get; set; }

        public string? German { get; set; }

        public string? Italian { get; set; }

        public string? Japanese { get; set; }

        public string? Korean { get; set; }

        public string? Polish { get; set; }

        public string? Russian { get; set; }

        public string? Spanish { get; set; }

        public string? Taiwanese { get; set; }


        public Localized(string? english = null)
        {
            if (english is not null)
            {
                Chinese = english;
                English = english;
                French = english;
                German = english;
                Italian = english;
                Japanese = english;
                Korean = english;
                Polish = english;
                Russian = english;
                Spanish = english;
                Taiwanese = english;
            }
        }
    }
}
